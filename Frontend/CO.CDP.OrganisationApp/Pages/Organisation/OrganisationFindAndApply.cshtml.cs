using System.Collections.Immutable;
using System.Text.RegularExpressions;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Logging;
using CO.CDP.OrganisationApp.WebApiClients;
using CO.CDP.UI.Foundation.Utilities;
using Microsoft.FeatureManagement.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

[ValidateAntiForgeryToken]
[Authorize(Policy = OrgScopeRequirement.Viewer)]
public partial class OrganisationFindAndApplyModel(
    IOrganisationClient organisationClient,
    ISession session,
    ILogger<OrganisationFindAndApplyModel> logger) : LoggedInUserAwareModel(session)
{
    public string BackLinkUrl { get; set; } = default!;

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)] public string? Origin { get; set; }

    private readonly ILogger<OrganisationFindAndApplyModel> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IActionResult> OnGet()
    {
        SetBackLinkUrl();
        return Page();
    }

    private void SetBackLinkUrl()
    {
        BackLinkUrl = Origin switch
        {
            "organisation-home" => $"/organisation/{Id}/home",
            _ => $"/organisation/{Id}/home"
        };
    }
}