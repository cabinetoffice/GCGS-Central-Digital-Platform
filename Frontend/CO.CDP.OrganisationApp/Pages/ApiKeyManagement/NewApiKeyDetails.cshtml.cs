using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.ApiKeyManagement;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class NewApiKeyDetailsModel(ISession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public string? ApiKey { get; private set; }

    public IActionResult OnGet()
    {
        // The key is handed over via the session so it never appears in the URL (and so in logs).
        // It is shown once; a refresh or direct navigation has no key to show.
        var apiKey = session.Get<string>(Session.NewApiKeyKey);
        if (string.IsNullOrEmpty(apiKey))
        {
            return Redirect($"/organisation/{Id}/manage-api-keys");
        }

        session.Remove(Session.NewApiKeyKey);
        ApiKey = apiKey;

        return Page();
    }
}