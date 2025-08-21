using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Extensions;
using CO.CDP.OrganisationApp.Logging;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement.Mvc;
using Address = CO.CDP.Organisation.WebApiClient.Address;

namespace CO.CDP.OrganisationApp.Pages.Buyer.Hierarchy;

[Authorize(Policy = PolicyNames.PartyRole.BuyerWithSignedMou)]
[Authorize(Policy = OrgScopeRequirement.Editor)]
[FeatureGate(FeatureFlags.BuyerParentChildRelationship)]
public class ChildOrganisationConfirmPage(
    IOrganisationClient organisationClient,
    ILogger<ChildOrganisationConfirmPage> logger)
    : PageModel
{
    private readonly IOrganisationClient _organisationClient =
        organisationClient ?? throw new ArgumentNullException(nameof(organisationClient));

    private readonly ILogger<ChildOrganisationConfirmPage> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)] public Guid ChildId { get; set; }

    [BindProperty(SupportsGet = true)] public string? Ppon { get; set; }

    [BindProperty(SupportsGet = true)] public string? Query { get; set; }

    [BindProperty] public string ChildOrganisationName { get; set; } = string.Empty;

    public string? WarningMessage { get; set; } = string.Empty;

    public string? WarningTagMessage { get; set; } = string.Empty;

    public CO.CDP.Organisation.WebApiClient.Organisation? ChildOrganisation { get; private set; }

    public Address? ChildOrganisationAddress => ChildOrganisation?.Addresses?.FirstOrDefault();

    public ContactPoint? ChildOrganisationContactPoint => ChildOrganisation?.ContactPoint;

    public string ChildOrganisationType =>
        ChildOrganisation?.Roles.GetDisplayText() ?? StaticTextResource.Global_Unknown;

    public bool HasChildOrganisationAddress =>
        ChildOrganisationAddress != null &&
        (!string.IsNullOrEmpty(ChildOrganisationAddress.StreetAddress) ||
         !string.IsNullOrEmpty(ChildOrganisationAddress.Locality) ||
         !string.IsNullOrEmpty(ChildOrganisationAddress.Region) ||
         !string.IsNullOrEmpty(ChildOrganisationAddress.PostalCode) ||
         !string.IsNullOrEmpty(ChildOrganisationAddress.CountryName));

    public async Task<IActionResult> OnGetAsync()
    {
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

            if (!ChildOrganisation.Roles.Contains(PartyRole.Buyer) &&
                !ChildOrganisation.Details.PendingRoles.Contains(PartyRole.Buyer))
            {
                _logger.LogWarning("Child organisation {ChildId} does not have a buyer role or pending buyer role associated", ChildId);
                return RedirectToPage("/Error");
            }

            ChildOrganisationName = ChildOrganisation.Name;

            if (IsChildPendingBuyer())
            {
                WarningTagMessage = @StaticTextResource
                    .BuyerParentChildRelationship_ConfirmPage_Tag_ApprovalPending;
                WarningMessage = @StaticTextResource
                    .BuyerParentChildRelationship_ConfirmPage_Warning_ApprovalPending;
            }
            else if (await IsChildConnectedAsParent())
            {
                WarningTagMessage = @StaticTextResource
                    .BuyerParentChildRelationship_ConfirmPage_Tag_ChildConnectedAsParent;
                WarningMessage = @StaticTextResource
                    .BuyerParentChildRelationship_ConfirmPage_Warning_ChildConnectedAsParent;
            }
            else if (await IsChildConnectedAsChild())
            {
                WarningTagMessage = @StaticTextResource
                    .BuyerParentChildRelationship_ConfirmPage_Tag_ChildConnectedAsChild;
                WarningMessage = @StaticTextResource
                    .BuyerParentChildRelationship_ConfirmPage_Warning_ChildConnectedAsChild;
            }
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

    private bool IsChildPendingBuyer()
    {
        if (ChildOrganisation == null)
        {
            return false;
        }

        if (ChildOrganisation.IsPendingBuyer())
        {
            _logger.LogInformation("Child organisation {ChildId} is pending buyer", ChildId);
            return true;
        }

        return false;
    }
    private async Task<bool> IsChildConnectedAsChild()
    {
        var connectedChildren = await _organisationClient.GetChildOrganisationsAsync(Id);
        if (connectedChildren == null || connectedChildren.Count == 0)
        {
            return false;
        }

        var childOrganisationMatch = connectedChildren.FirstOrDefault(x => x.Id == ChildId);
        if (childOrganisationMatch == null)
        {
            return false;
        }

        _logger.LogInformation("Child organisation {ChildId} is already assigned to parent {ParentId}",
            ChildId, Id);

        return true;
    }

    private async Task<bool> IsChildConnectedAsParent()
    {
        var connectedParents = await _organisationClient.GetParentOrganisationsAsync(Id);
        if (connectedParents == null || connectedParents.Count == 0)
        {
            return false;
        }

        var parentOrganisationMatch = connectedParents.FirstOrDefault(x => x.Id == ChildId);
        if (parentOrganisationMatch == null)
        {
            return false;
        }

        _logger.LogInformation(
            "Child organisation {ChildId} is already a parent to the organisation for the parent {ParentId}",
            ChildId, Id);
        return true;
    }

    public async Task<IActionResult> OnPostAsync()
    {
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