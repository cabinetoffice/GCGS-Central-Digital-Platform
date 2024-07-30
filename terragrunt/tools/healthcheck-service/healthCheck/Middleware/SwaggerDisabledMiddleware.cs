using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace YourProject.Middleware
{
    public class SwaggerDisabledMiddleware
    {
        private readonly RequestDelegate _next;

        public SwaggerDisabledMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync("<html><body><h1>Swagger UI is disabled in this environment.</h1></body></html>");
            }
            else
            {
                await _next(context);
            }
        }
    }
}
