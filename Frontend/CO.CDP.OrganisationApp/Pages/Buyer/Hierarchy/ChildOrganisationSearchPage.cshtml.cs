using System.ComponentModel.DataAnnotations;
using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Authorization;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Buyer.Hierarchy;

[Authorize(Policy = PartyRoleRequirement.Buyer)]
[Authorize(Policy = OrgScopeRequirement.Editor)]
[FeatureGate(FeatureFlags.BuyerParentChildRelationship)]
public class ChildOrganisationSearchPage(IFeatureManager featureManager, IAuthorizationService authorizationService, ILogger<ChildOrganisationSearchPage> logger)
    : PageModel
{
    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }

    [BindProperty]
    [Required(ErrorMessage = nameof(StaticTextResource.BuyerParentChildRelationship_SearchPage_Error))]
    public string? Query { get; set; }

    public bool SearchRegistryPponEnabled { get; private set; }

    private readonly ILogger<ChildOrganisationSearchPage> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IAuthorizationService _authorizationService =
        authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));

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