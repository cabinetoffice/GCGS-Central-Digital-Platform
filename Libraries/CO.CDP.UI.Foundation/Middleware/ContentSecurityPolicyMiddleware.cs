using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;

namespace CO.CDP.UI.Foundation.Middleware;

public class ContentSecurityPolicyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ContentSecurityPolicyOptions _options;

    public ContentSecurityPolicyMiddleware(RequestDelegate next, ContentSecurityPolicyOptions options)
    {
        _next = next;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string nonce = GenerateNonce();
        context.Items["ContentSecurityPolicyNonce"] = nonce;

        context.Response.Headers.Append("Content-Security-Policy", _options.BuildPolicy(nonce));

        await _next(context);
    }

    private static string GenerateNonce()
    {
        var nonceBytes = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(nonceBytes);
        return Convert.ToBase64String(nonceBytes);
    }
}
