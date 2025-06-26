using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Logging;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Address = CO.CDP.Organisation.WebApiClient.Address;

namespace CO.CDP.OrganisationApp.Pages.BuyerParentChildRelationship;

public class ChildOrganisationConfirmPage : PageModel
{
    private readonly IOrganisationClient _organisationClient;
    private readonly ILogger<ChildOrganisationConfirmPage> _logger;

    public ChildOrganisationConfirmPage(
        IOrganisationClient organisationClient,
        ILogger<ChildOrganisationConfirmPage> logger)
    {
        _organisationClient = organisationClient ?? throw new ArgumentNullException(nameof(organisationClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)] public Guid ChildId { get; set; }

    [BindProperty(SupportsGet = true)] public string? Query { get; set; }

    public CO.CDP.Organisation.WebApiClient.Organisation? ChildOrganisation { get; private set; }

    public Address? OrganisationAddress => ChildOrganisation?.Addresses?.FirstOrDefault();
    public ContactPoint? OrganisationContactPoint => ChildOrganisation?.ContactPoint;
    public string OrganisationType => "Buyer";

    public async Task<IActionResult> OnGetAsync()
    {
        if (ChildId == Guid.Empty)
        {
            return RedirectToPage("ChildOrganisationSearchPage", new { Id });
        }

        try
        {
            ChildOrganisation = await _organisationClient.GetOrganisationAsync(ChildId);

            if (ChildOrganisation == null)
            {
                _logger.LogWarning("Organisation not found for ChildId: {ChildId}", ChildId);
                return RedirectToPage("/Error");
            }

            _logger.LogInformation("Retrieved child organisation: {OrganisationName}", ChildOrganisation.Name);
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
        try
        {
            if (ChildOrganisation == null)
            {
                _logger.LogWarning("Child organisation not found for ID: {ChildId}", ChildId);
                return RedirectToPage("/Error");
            }

            _logger.LogInformation("Creating relationship between parent ID: {ParentId} and child ID: {ChildId}",
                Id, ChildOrganisation.Id);

            var request = new CreateParentChildRelationshipRequest(
                Id,
                ChildOrganisation.Id,
                PartyRole.Buyer
            );

            await _organisationClient.CreateParentChildRelationshipAsync(request);

            return RedirectToPage("ChildOrganisationSuccessPage",
                new { Id, OrganisationName = ChildOrganisation.Name });
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