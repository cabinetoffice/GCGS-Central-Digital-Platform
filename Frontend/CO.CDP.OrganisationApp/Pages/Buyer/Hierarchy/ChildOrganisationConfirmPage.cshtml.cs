using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Authorization;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Extensions;
using CO.CDP.OrganisationApp.Logging;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Address = CO.CDP.Organisation.WebApiClient.Address;

namespace CO.CDP.OrganisationApp.Pages.Buyer.Hierarchy;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class ChildOrganisationConfirmPage(
    IOrganisationClient organisationClient,
    ILogger<ChildOrganisationConfirmPage> logger,
    IAuthorizationService authorizationService)
    : PageModel
{
    private readonly IOrganisationClient _organisationClient =
        organisationClient ?? throw new ArgumentNullException(nameof(organisationClient));

    private readonly ILogger<ChildOrganisationConfirmPage> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IAuthorizationService _authorizationService =
        authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)] public Guid ChildId { get; set; }

    [BindProperty(SupportsGet = true)] public string? Ppon { get; set; }

    [BindProperty(SupportsGet = true)] public string? Query { get; set; }

    [BindProperty] public string ChildOrganisationName { get; set; } = string.Empty;

    public CO.CDP.Organisation.WebApiClient.Organisation? ChildOrganisation { get; private set; }

    public Address? ChildOrganisationAddress => ChildOrganisation?.Addresses?.FirstOrDefault();
    public ContactPoint? ChildOrganisationContactPoint => ChildOrganisation?.ContactPoint;
    public string ChildOrganisationType => ChildOrganisation?.Roles.GetDisplayText() ?? StaticTextResource.Global_Unknown;

    public bool HasChildOrganisationAddress =>
        ChildOrganisationAddress != null &&
        (!string.IsNullOrEmpty(ChildOrganisationAddress.StreetAddress) ||
         !string.IsNullOrEmpty(ChildOrganisationAddress.Locality) ||
         !string.IsNullOrEmpty(ChildOrganisationAddress.Region) ||
         !string.IsNullOrEmpty(ChildOrganisationAddress.PostalCode) ||
         !string.IsNullOrEmpty(ChildOrganisationAddress.CountryName));

    public async Task<IActionResult> OnGetAsync()
    {
        var authResult = await _authorizationService.AuthorizeAsync(User, Id, new IsBuyerRequirement());
        if (!authResult.Succeeded)
        {
            _logger.LogWarning("User is not authorised to access child organisation search for parent ID {OrganisationId}.", Id);
            return Redirect("/page-not-found");
        }

        if (ChildId == Guid.Empty)
        {
            return RedirectToPage("ChildOrganisationSearchPage", new { Id });
        }

        try
        {
            ChildOrganisation = await OrganisationClientExtensions.LookupOrganisationAsync(_organisationClient,
                name: null,
                identifier: Ppon);

            if (ChildOrganisation == null)
            {
                _logger.LogWarning("Organisation not found for ChildId: {ChildId}", ChildId);
                return RedirectToPage("/Error");
            }

            ChildOrganisationName = ChildOrganisation.Name;
        }
        catch (Exception ex)
        {
            var errorMessage = "Error occurred while retrieving organisation details";
            var cdpException = new CdpExceptionLogging(errorMessage, "LOOKUP_ERROR", ex);
            _logger.LogError(cdpException, errorMessage);
            return RedirectToPage("/Error");
        }

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

        try
        {
            _logger.LogInformation("Creating relationship between parent ID: {ParentId} and child ID: {ChildId}",
                Id, ChildId);

            var request = new CreateParentChildRelationshipRequest(
                parentId: Id,
                childId: ChildId
            );

            await _organisationClient.CreateParentChildRelationshipAsync(Id, request);

            return RedirectToPage("ChildOrganisationSuccessPage",
                new { Id, ChildId, OrganisationName = ChildOrganisationName });
        }
        catch (Exception ex)
        {
            var errorMessage = "Error occurred while establishing parent-child relationship";
            var cdpException = new CdpExceptionLogging(errorMessage, "RELATIONSHIP_ERROR", ex);
            _logger.LogError(cdpException, errorMessage);
            return RedirectToPage("/Error");
        }
    }
}