namespace CO.CDP.RegisterOfCommercialTools.WebApi.Middleware;

public class ExceptionMiddleware(
    RequestDelegate next,
    ILogger<ExceptionMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next.Invoke(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Request failed: {Message}", ex.Message);
            throw;
        }
    }
}