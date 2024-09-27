using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.ApiKeyManagement;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class NewApiKeyDetailsModel() : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ApiKey { get; set; }

    public IActionResult OnGet()
    {        
        return Page();
    }
}