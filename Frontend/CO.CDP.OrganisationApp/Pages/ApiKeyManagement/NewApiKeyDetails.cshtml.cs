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

    public const string TempDataKey = "NewApiKey";

    public string? ApiKey { get; private set; }

    public IActionResult OnGet()
    {
        // The key is handed over via TempData so it never appears in the URL (and so in logs).
        // It is shown once; a refresh or direct navigation has no key to show.
        if (TempData[TempDataKey] is not string apiKey || string.IsNullOrEmpty(apiKey))
        {
            return Redirect($"/organisation/{Id}/manage-api-keys");
        }

        TempData.Remove(TempDataKey);
        ApiKey = apiKey;

        return Page();
    }
}