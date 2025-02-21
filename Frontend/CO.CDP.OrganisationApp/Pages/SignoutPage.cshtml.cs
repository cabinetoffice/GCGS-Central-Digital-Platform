using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement;

namespace CO.CDP.OrganisationApp.Pages;

[AuthenticatedSessionNotRequired]
public class SignedOutModel(
    IFeatureManager featureManager,
    IFtsUrlService ftsUrlService) : PageModel
{
    public string HomePageLink { get; set; } = "/";

    public async Task<IActionResult> OnGet()
    {
        if (await featureManager.IsEnabledAsync(FeatureFlags.AllowFtsRedirectLinks))
        {
            HomePageLink = ftsUrlService.BuildUrl("");
        }

        return Page();
    }
}