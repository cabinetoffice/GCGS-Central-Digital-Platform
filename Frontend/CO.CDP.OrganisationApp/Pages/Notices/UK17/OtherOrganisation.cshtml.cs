using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.OrganisationApp.Logging;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.FeatureManagement.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Notices.UK17;

[ValidateAntiForgeryToken]
[Authorize(Policy = PolicyNames.PartyRole.BuyerWithSignedMou)]
[Authorize(Policy = OrgScopeRequirement.Viewer)]
[FeatureGate(FeatureFlags.SearchRegistryPpon)]
public partial class OtherOrganisationModel(
    IOrganisationClient organisationClient,
    ISession session,
    ILogger<OtherOrganisationModel> logger) : LoggedInUserAwareModel(session)
{
    private readonly ILogger<OtherOrganisationModel> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public string? ErrorMessage { get; set; }

    public string BackLinkUrl { get; set; } = default!;

    public async Task<IActionResult> OnGet()
    {
        return Page();
    }
}
