using Microsoft.AspNetCore.Razor.TagHelpers;

[HtmlTargetElement("script", Attributes = "nonce-csp")]
[HtmlTargetElement("style", Attributes = "nonce-csp")]
public class ContentSecurityPolicyNonceTagHelper : TagHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ContentSecurityPolicyNonceTagHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext != null && httpContext.Items["ContentSecurityPolicyNonce"] is string nonce)
        {
            output.Attributes.SetAttribute("nonce", nonce);
        }

        output.Attributes.RemoveAll("nonce-csp");
    }
}
