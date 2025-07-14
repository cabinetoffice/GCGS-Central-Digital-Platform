using System.ComponentModel.DataAnnotations;
using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement;

namespace CO.CDP.OrganisationApp.Pages.Buyer.Hierarchy;

public class ChildOrganisationSearchPage(IFeatureManager featureManager) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    [Required(ErrorMessage = nameof(StaticTextResource.BuyerParentChildRelationship_SearchPage_Error))]
    public string? Query { get; set; }

    public bool SearchRegistryPponEnabled { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        SearchRegistryPponEnabled = await featureManager.IsEnabledAsync(FeatureFlags.SearchRegistryPpon);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            SearchRegistryPponEnabled = await featureManager.IsEnabledAsync(FeatureFlags.SearchRegistryPpon);
            return Page();
        }

        return RedirectToPage("ChildOrganisationResultsPage", new { Id, query = Query });
    }
}