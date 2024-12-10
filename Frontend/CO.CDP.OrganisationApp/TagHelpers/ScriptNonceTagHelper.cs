using Microsoft.AspNetCore.Razor.TagHelpers;

[HtmlTargetElement("script", Attributes = "nonce-csp")]
public class ScriptNonceTagHelper : TagHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ScriptNonceTagHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        if (httpContext.Items["ContentSecurityPolicyNonce"] is string nonce)
        {
            output.Attributes.SetAttribute("nonce", nonce);
        }

        output.Attributes.RemoveAll("nonce-csp");
    }
}
