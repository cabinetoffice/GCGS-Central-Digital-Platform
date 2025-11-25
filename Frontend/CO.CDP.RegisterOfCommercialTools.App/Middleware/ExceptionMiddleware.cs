namespace CO.CDP.RegisterOfCommercialTools.App.Middleware;

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
        catch (HttpRequestException hex) when (hex.StatusCode.HasValue && (int)hex.StatusCode >= 500)
        {
            logger.LogError(hex, "Server error ({StatusCode}) received from API", hex.StatusCode);
            context.Response.Redirect("/error");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            context.Response.Redirect("/error");
        }
    }
}