using System.ComponentModel.DataAnnotations;
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
using Microsoft.FeatureManagement.Mvc;
using OrganisationApiException = CO.CDP.Organisation.WebApiClient.ApiException;

namespace CO.CDP.OrganisationApp.Pages.Buyer.Hierarchy;

[Authorize(Policy = PartyRoleRequirement.Buyer)]
[Authorize(Policy = OrgScopeRequirement.Editor)]
[FeatureGate(FeatureFlags.BuyerParentChildRelationship)]
public class ChildOrganisationRemovePage(
    IOrganisationClient organisationClient,
    ILogger<ChildOrganisationRemovePage> logger)
    : PageModel
{
    private readonly IOrganisationClient _organisationClient =
        organisationClient ?? throw new ArgumentNullException(nameof(organisationClient));

    private readonly ILogger<ChildOrganisationRemovePage> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)] public Guid ChildId { get; set; }

    [BindProperty(SupportsGet = true)] public string? Ppon { get; set; }

    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Global_PleaseSelect),
        ErrorMessageResourceType = typeof(StaticTextResource))]
    public bool? RemoveConfirmation { get; set; }

    public bool HasValidationErrors => !ModelState.IsValid;

    public CO.CDP.Organisation.WebApiClient.Organisation? ChildOrganisation { get; set; }

    public Address? ChildOrganisationAddress => ChildOrganisation?.Addresses?.FirstOrDefault();

    public bool HasChildOrganisationAddress =>
        ChildOrganisationAddress != null &&
        (!string.IsNullOrEmpty(ChildOrganisationAddress.StreetAddress) ||
         !string.IsNullOrEmpty(ChildOrganisationAddress.Locality) ||
         !string.IsNullOrEmpty(ChildOrganisationAddress.Region) ||
         !string.IsNullOrEmpty(ChildOrganisationAddress.PostalCode) ||
         !string.IsNullOrEmpty(ChildOrganisationAddress.CountryName));

    public async Task<IActionResult> OnGet()
    {
        try
        {
            ChildOrganisation = await OrganisationClientExtensions.LookupOrganisationAsync(_organisationClient,
                name: null,
                identifier: Ppon);

            if (ChildOrganisation == null)
            {
                _logger.LogWarning("Organisation not found for ChildId: {ChildId}", ChildId);
                return Redirect("/page-not-found");
            }

            var (hasPpon, _) = ChildOrganisation.GetGbPponIdentifier();
            if (!hasPpon)
            {
                _logger.LogWarning("Organisation does not have a GB-PPON identifier for ChildId: {ChildId}", ChildId);
                return Redirect("/page-not-found");
            }

            var childOrganisations = await OrganisationClientExtensions.GetChildOrganisationsAsync(_organisationClient, Id);
            if (childOrganisations.Any(o => o.Id == ChildId)) return Page();

            _logger.LogWarning("Child organisation with ID {ChildId} is not a child of organisation with ID {ParentId}",
                ChildId, Id);
            return Redirect("/page-not-found");
        }
        catch (OrganisationApiException ex)
        {
            var errorMessage =
                $"Error occurred while retrieving child organisation details for parent ID: {Id}, child ID: {ChildId}";
            var cdpException = new CdpExceptionLogging(errorMessage, "CHILD_ORG_LOOKUP_ERROR", ex);
            _logger.LogError(cdpException, errorMessage);
            return RedirectToPage("/Error");
        }
    }

    public string? GetChildOrganisationPpon()
    {
        return ChildOrganisation?.GetPponValue();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (RemoveConfirmation == true)
        {
            return await Delete();
        }

        return RedirectToPage("/Organisation/OrganisationOverview", new { id = Id });
    }

    private async Task<IActionResult> Delete()
    {
        try
        {
            await _organisationClient.SupersedeChildOrganisationAsync(Id, ChildId);
            return RedirectToPage("/Organisation/OrganisationOverview", new { id = Id, childRemoved = true });
        }
        catch (OrganisationApiException ex)
        {
            var errorMessage =
                $"Failed to remove child organisation with ID: {ChildId} from parent organisation with ID: {Id}";
            var cdpException = new CdpExceptionLogging(errorMessage, "CHILD_ORG_REMOVE_ERROR", ex);
            _logger.LogError(cdpException, errorMessage);

            return RedirectToPage("/Error");
        }
    }
}