using Microsoft.AspNetCore.Builder;

namespace CO.CDP.UI.Foundation.Middleware;

public static class ContentSecurityPolicyMiddlewareExtensions
{
    public static IApplicationBuilder UseContentSecurityPolicy(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ContentSecurityPolicyMiddleware>();
    }
}
