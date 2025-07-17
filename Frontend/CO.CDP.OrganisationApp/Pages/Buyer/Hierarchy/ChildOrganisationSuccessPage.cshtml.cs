using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Authorization;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Buyer.Hierarchy;

[Authorize(Policy = PartyRoleRequirement.Buyer)]
[Authorize(Policy = OrgScopeRequirement.Editor)]
[FeatureGate(FeatureFlags.BuyerParentChildRelationship)]
public class ChildOrganisationSuccessPage(
    IOrganisationClient organisationClient,
    ILogger<ChildOrganisationSuccessPage> logger)
    : PageModel
{
    private readonly IOrganisationClient _organisationClient =
        organisationClient ?? throw new ArgumentNullException(nameof(organisationClient));

    private readonly ILogger<ChildOrganisationSuccessPage> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    [BindProperty(SupportsGet = true)] public required Guid Id { get; set; }

    [BindProperty(SupportsGet = true)] public required Guid ChildId { get; set; }

    [BindProperty(SupportsGet = true)] public string OrganisationName { get; set; } = string.Empty;

    public CO.CDP.Organisation.WebApiClient.Organisation? ChildOrganisation { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (ChildId == Guid.Empty)
        {
            return RedirectToPage("/Error");
        }

        try
        {
            ChildOrganisation = await _organisationClient.GetOrganisationAsync(ChildId);

            if (ChildOrganisation == null)
            {
                _logger.LogWarning("Child organisation not found for ChildId: {ChildId}", ChildId);
                return RedirectToPage("/Error");
            }

            OrganisationName = ChildOrganisation.Name;
        }
        catch (Exception ex)
        {
            var errorMessage = "Error occurred while retrieving child organisation details";
            var cdpException = new CdpExceptionLogging(errorMessage, "LOOKUP_ERROR", ex);
            _logger.LogError(cdpException, errorMessage);
            return RedirectToPage("/Error");
        }

        return Page();
    }
}