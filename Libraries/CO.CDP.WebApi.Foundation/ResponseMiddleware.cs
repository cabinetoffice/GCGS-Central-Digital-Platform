using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CO.CDP.WebApi.Foundation;

internal class ResponseMiddleware(
        RequestDelegate next,
        ILogger<ResponseMiddleware> logger,
        IWebHostEnvironment webHostEnvironment,
        Dictionary<Type, (int, string)> exceptionMap)
{
    public async Task Invoke(HttpContext context)
    {
        var request = $"{context.Request.Method} {context.Request.Path}";

        try
        {
            await next.Invoke(context);

            var statusCode = context.Response.StatusCode;

            if (statusCode >= 400 && statusCode < 500)
            {
                logger.LogInformation("Response status: {statusCode}, for request: {request}", statusCode, request);

                await HandleResponse(context, statusCode, new ProblemDetails { Status = statusCode });
            }
        }
        catch (Exception ex)
        {
            await HandleException(context, request, ex);
        }
    }

    private async Task HandleException(HttpContext context, string request, Exception ex)
    {
        var statusCode = StatusCodes.Status500InternalServerError;
        var errorCode = "GENERIC_ERROR";
        var message = "An unexpected error has occurred";

        if (exceptionMap.TryGetValue(ex.GetType(), out (int status, string code) error))
        {
            statusCode = error.status;
            message = ex.Message;
            errorCode = error.code;
            logger.LogInformation(ex, "Response status: {statusCode}, for request: {requestUrl}", statusCode, request);
        }
        else
        {
            logger.LogError(ex, ex.Message);
        }

        var pd = new ProblemDetails
        {
            Status = statusCode,
            Detail = webHostEnvironment.IsDevelopment() ? ex.ToString() : message
        };
        pd.Extensions.Add("code", errorCode);

        await HandleResponse(context, statusCode, pd);
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

            pd.Instance = context.Request.Path;
            pd.Extensions.Add("trace-id", context.TraceIdentifier);

            await problemDetailsService.WriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext = context,
                    ProblemDetails = pd
                });
        }
    }
}