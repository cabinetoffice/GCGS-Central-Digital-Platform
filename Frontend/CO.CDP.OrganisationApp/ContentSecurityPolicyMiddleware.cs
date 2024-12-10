using System.Security.Cryptography;

public class ContentSecurityPolicyMiddleware
{
    private readonly RequestDelegate _next;

    public ContentSecurityPolicyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("Features:ContentSecurityPolicy"))
        {
            string nonce = GenerateNonce();
            context.Items["ContentSecurityPolicyNonce"] = nonce;

            context.Response.Headers.Append("Content-Security-Policy",
                "default-src 'self'; " +
                $"script-src 'self' https://*.googletagmanager.com 'nonce-{nonce}'; " +
                "img-src 'self' https://*.google-analytics.com https://*.googletagmanager.com; " +
                "connect-src 'self' https://*.google-analytics.com https://*.analytics.google.com https://*.googletagmanager.com");
        }

        await _next(context);
    }

    private string GenerateNonce()
    {
        var nonceBytes = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(nonceBytes);
        return Convert.ToBase64String(nonceBytes);
    }
}
