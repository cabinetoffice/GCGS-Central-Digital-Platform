using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CO.CDP.UI.Foundation.TagHelpers;

[HtmlTargetElement("script", Attributes = "nonce-csp")]
[HtmlTargetElement("style", Attributes = "nonce-csp")]
public class ContentSecurityPolicyNonceTagHelper(IHttpContextAccessor httpContextAccessor) : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext != null && httpContext.Items["ContentSecurityPolicyNonce"] is string nonce)
        {
            output.Attributes.SetAttribute("nonce", nonce);
        }

        output.Attributes.RemoveAll("nonce-csp");
    }
}
