using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Buyer.Hierarchy;

[Authorize(Policy = PolicyNames.PartyRole.BuyerWithSignedMou)]
[Authorize(Policy = OrgScopeRequirement.Editor)]
[FeatureGate(FeatureFlags.BuyerParentChildRelationship)]
public class ChildOrganisationSuccessPage(ILogger<ChildOrganisationSuccessPage> logger)
    : PageModel
{
    private readonly ILogger<ChildOrganisationSuccessPage> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    [BindProperty(SupportsGet = true)] public required Guid Id { get; set; }

    public string ChildName { get; private set; } = string.Empty;

    public Task<IActionResult> OnGetAsync()
    {
        if (TempData["ChildName"] is not string childName)
        {
            _logger.LogWarning("Child organisation name not found in TempData");
            return Task.FromResult<IActionResult>(RedirectToPage("/Error"));
        }

        ChildName = childName;
        TempData.Remove("ChildName");

        return Task.FromResult<IActionResult>(Page());
    }
}