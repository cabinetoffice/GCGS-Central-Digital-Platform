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

    [BindProperty] public string? OrganisationName { get; set; }

    public ChildOrganisation? ChildOrganisation { get; set; }

    public Address? OrganisationAddress { get; set; }
    public ContactPoint? OrganisationContactPoint { get; set; }
    public string OrganisationType => "Buyer";

    public async Task<IActionResult> OnGetAsync()
    {
        if (ChildId == Guid.Empty)
        {
            return RedirectToPage("ChildOrganisationSearchPage", new { Id });
        }

        try
        {
            var organisation = await _organisationClient.GetOrganisationAsync(ChildId);

            if (organisation == null)
            {
                _logger.LogWarning("Organisation not found for ChildId: {ChildId}", ChildId);
                return RedirectToPage("/Error");
            }

            ChildOrganisation = new ChildOrganisation(
                organisation.Name,
                organisation.Id,
                organisation.Identifier
            );
            OrganisationName = organisation.Name;

            OrganisationAddress = organisation.Addresses?.FirstOrDefault();
            OrganisationContactPoint = organisation.ContactPoint;
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
            var request = new CreateParentChildRelationshipRequest
            (
                ChildId,
                Id,
                PartyRole.Buyer
            );

            await _organisationClient.CreateParentChildRelationshipAsync(request);

            return RedirectToPage("ChildOrganisationSuccessPage", new { Id, OrganisationName });
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