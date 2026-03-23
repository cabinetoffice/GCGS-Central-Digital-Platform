using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Logging;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.FeatureManagement.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

[ValidateAntiForgeryToken]
[Authorize(Policy = OrgScopeRequirement.Viewer)]
public partial class OrganisationFindAndApplyModel(
    IOrganisationClient organisationClient,
    ISession session,
    IConfiguration configuration,
    ILogger<OrganisationFindAndApplyModel> logger) : LoggedInUserAwareModel(session)
{
    public string BackLinkUrl { get; set; } = default!;

    public string FindAGrantUrl { get; private set; } = default!;

    public string ManageAGrantUrl { get; private set; } = default!;

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)] public string? Origin { get; set; }

    private readonly ILogger<OrganisationFindAndApplyModel> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IActionResult> OnGet()
    {
        SetBackLinkUrl();
        FindAGrantUrl = configuration["FindAndApplyApp:FindAGrantUrl"]
            ?? throw new InvalidOperationException("FindAndApplyApp:FindAGrantUrl is not configured.");
        ManageAGrantUrl = configuration["FindAndApplyApp:ManageAGrantUrl"]
            ?? throw new InvalidOperationException("FindAndApplyApp:ManageAGrantUrl is not configured.");
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