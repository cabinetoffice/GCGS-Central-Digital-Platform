using Microsoft.AspNetCore.Http;

public class CacheControlMiddleware
{
    private readonly RequestDelegate _next;

    public CacheControlMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey("Cache-Control"))
            {
                context.Response.Headers["Cache-Control"] = "private, no-store, no-cache, must-revalidate";
            }
            return Task.CompletedTask;
        });

        await _next(context);
    }
}