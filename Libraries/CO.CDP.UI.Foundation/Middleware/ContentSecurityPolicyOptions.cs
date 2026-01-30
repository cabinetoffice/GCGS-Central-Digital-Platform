namespace CO.CDP.UI.Foundation.Middleware;

public class ContentSecurityPolicyOptions
{
    public string DefaultSrc { get; set; } = "'self'";
    public string StyleSrc { get; set; } = "'self'";
    public string ScriptSrc { get; set; } = "'self' https://*.googletagmanager.com";
    public string ImgSrc { get; set; } = "'self' https://*.google-analytics.com https://*.googletagmanager.com";
    public string ConnectSrc { get; set; } = "'self' https://*.google-analytics.com https://*.analytics.google.com https://*.googletagmanager.com";

    public string BuildPolicy(string nonce)
    {
        return $"default-src {DefaultSrc}; " +
               $"style-src {StyleSrc} 'nonce-{nonce}'; " +
               $"script-src {ScriptSrc} 'nonce-{nonce}'; " +
               $"img-src {ImgSrc}; " +
               $"connect-src {ConnectSrc}";
    }
}
