using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement;

namespace CO.CDP.OrganisationApp.Pages;

[AuthenticatedSessionNotRequired]
public class IndexModel(
    IFeatureManager featureManager,
    IConfiguration config) : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        if (await featureManager.IsEnabledAsync(FeatureFlags.RedirectToFtsHomepage))
        {
            var ftsHomepage = config["FtsService"];
            if (!string.IsNullOrEmpty(ftsHomepage))
            {
                return Redirect(ftsHomepage);
            }
        }

        return Page();
    }
}