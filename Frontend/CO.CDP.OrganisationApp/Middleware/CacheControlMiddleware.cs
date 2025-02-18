using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;

public class CacheControlMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IContentTypeProvider _contentTypeProvider;


    public CacheControlMiddleware(RequestDelegate next)
    {
        _next = next;
        _contentTypeProvider = new FileExtensionContentTypeProvider();

    }

    public async Task Invoke(HttpContext context)
    {
        if (IsStaticFileRequest(context.Request.Path))
        {
            await _next(context);
            return;
        }
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

    private bool IsStaticFileRequest(PathString path)
    {
        var fileExtension = System.IO.Path.GetExtension(path);
        return fileExtension != null && _contentTypeProvider.TryGetContentType(fileExtension, out _);
    }
}