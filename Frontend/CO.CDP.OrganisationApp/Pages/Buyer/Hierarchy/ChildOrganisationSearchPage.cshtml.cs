using System.ComponentModel.DataAnnotations;
using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Authorization;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement;

namespace CO.CDP.OrganisationApp.Pages.Buyer.Hierarchy;

[Authorize(Policy = OrgScopeRequirement.Editor)]
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
        var authResult = await _authorizationService.AuthorizeAsync(User, Id, new IsBuyerRequirement());
        if (!authResult.Succeeded)
        {
            _logger.LogWarning("User is not authorised to access child organisation search for parent ID {OrganisationId}.", Id);
            return Redirect("/page-not-found");
        }

        SearchRegistryPponEnabled = await featureManager.IsEnabledAsync(FeatureFlags.SearchRegistryPpon);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var authResult = await _authorizationService.AuthorizeAsync(User, Id, new IsBuyerRequirement());
        if (!authResult.Succeeded)
        {
            _logger.LogWarning("User is not authorised to access child organisation search for parent ID {OrganisationId}.", Id);
            return Redirect("/page-not-found");
        }

        if (!ModelState.IsValid)
        {
            SearchRegistryPponEnabled = await featureManager.IsEnabledAsync(FeatureFlags.SearchRegistryPpon);
            return Page();
        }

        return RedirectToPage("ChildOrganisationResultsPage", new { Id, query = Query });
    }
}