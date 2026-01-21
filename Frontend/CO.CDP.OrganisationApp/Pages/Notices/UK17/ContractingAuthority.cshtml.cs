using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Logging;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using CO.CDP.UI.Foundation.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace CO.CDP.OrganisationApp.Pages.Notices.UK17;

[ValidateAntiForgeryToken]
[Authorize(Policy = PolicyNames.PartyRole.BuyerWithSignedMou)]
[Authorize(Policy = OrgScopeRequirement.Viewer)]
[FeatureGate(FeatureFlags.SearchRegistryPpon)]
public partial class ContractingAuthorityModel(
    IOrganisationClient organisationClient,
    ISession session,
    ILogger<ContractingAuthorityModel> logger) : LoggedInUserAwareModel(session)
{
    private readonly ILogger<ContractingAuthorityModel> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public string? ErrorMessage { get; set; }

    public string BackLinkUrl { get; set; } = default!;

    public async Task<IActionResult> OnGet()
    {
        return Page();
    }

    public IActionResult OnPost()
    {
        return RedirectToPage("OtherOrganisation");
    }
}