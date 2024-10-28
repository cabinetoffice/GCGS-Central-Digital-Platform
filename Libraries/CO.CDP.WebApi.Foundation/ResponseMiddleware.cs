using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CO.CDP.WebApi.Foundation;

internal class ResponseMiddleware(
        RequestDelegate next,
        ILogger<ResponseMiddleware> logger,
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
            var statusCode = StatusCodes.Status500InternalServerError;
            var errorCode = "GENERIC_ERROR";
            var pd = new ProblemDetails { Status = statusCode };

            if (exceptionMap.TryGetValue(ex.GetType(), out (int status, string code) error))
            {
                errorCode = error.code;
                pd.Status = statusCode = error.status;
                pd.Detail = ex.Message;
                logger.LogInformation(ex, "Response status: {statusCode}, for request: {requestUrl}", statusCode, requestUrl);
            }
            else
            {
                logger.LogError(ex, ex.Message);
            }

            pd.Extensions.Add("code", errorCode);

            await HandleResponse(context, statusCode, pd);
        }
    }

    private static async Task HandleResponse(HttpContext context, int statusCode, ProblemDetails pd)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

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
}