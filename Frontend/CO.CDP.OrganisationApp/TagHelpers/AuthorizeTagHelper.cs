using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CO.CDP.OrganisationApp.TagHelpers;

public class AuthorizeTagHelper: TagHelper
{
    public required string Scope { get; set; }
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizeTagHelper(IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;
        output.Content.SetContent(string.Empty);

        var user = _httpContextAccessor?.HttpContext?.User;
        if (user != null)
        {
            var roleCheck = await _authorizationService.AuthorizeAsync(user, Scope);
            if (roleCheck.Succeeded)
            {
                output.Content.SetHtmlContent(output.GetChildContentAsync().Result.GetContent());
            }
        }
    }
}
