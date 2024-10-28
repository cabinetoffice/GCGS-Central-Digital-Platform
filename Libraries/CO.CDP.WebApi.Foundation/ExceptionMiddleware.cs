using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace CO.CDP.WebApi.Foundation;

public class ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        Dictionary<Type, (int, string)> exceptionMap)
{
    public async Task Invoke(HttpContext context)
    {
        var requestUrl = context.Request.GetDisplayUrl();

        try
        {
            await next.Invoke(context);

            var statusCode = context.Response.StatusCode;

            if (statusCode >= 400 && statusCode < 500)
            {
                logger.LogInformation("Response status: {statusCode}, for request: {requestUrl}", statusCode, requestUrl);

                await HandleResponse(context, statusCode, new ProblemDetails { Status = statusCode });
            }
        }
        catch (Exception ex)
        {
            var (statusCode, errorCode) = MapException(ex);

            var pd = new ProblemDetails { Status = statusCode };
            pd.Extensions.Add("code", errorCode);

            logger.LogInformation(ex, "Response status: {statusCode}, for request: {requestUrl}", statusCode, requestUrl);

            await HandleResponse(context, statusCode, pd);
        }
    }

    private static async Task HandleResponse(HttpContext context, int statusCode, ProblemDetails pd)
    {
        if (context.RequestServices.GetService<IProblemDetailsService>() is { } problemDetailsService)
        {
            context.Response.StatusCode = statusCode;

            await problemDetailsService.WriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext = context,
                    ProblemDetails = pd
                });
        }
    }

    private (int status, string error) MapException(Exception exception)
    {
        if (exceptionMap.TryGetValue(exception.GetType(), out (int, string) code))
        {
            return code;
        }

        return (StatusCodes.Status500InternalServerError, "GENERIC_ERROR");
    }
}