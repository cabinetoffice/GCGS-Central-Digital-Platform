using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Admin;

[Authorize(Policy = PersonScopeRequirement.SuperAdmin)]
public class AppRegistryAdminModel : PageModel
{
    public string? OrganisationApiBaseUrl { get; private set; }

    public void OnGet()
    {
        OrganisationApiBaseUrl = HttpContext.Request.Scheme
            + "://"
            + HttpContext.Request.Host;
    }
}
