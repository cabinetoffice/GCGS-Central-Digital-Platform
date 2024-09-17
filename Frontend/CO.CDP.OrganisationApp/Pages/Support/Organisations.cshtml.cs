using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using PartyRole = CO.CDP.Organisation.WebApiClient.PartyRole;
using UserDetails = CO.CDP.OrganisationApp.Models.UserDetails;

namespace CO.CDP.OrganisationApp.Pages.Support;

public class OrganisationsModel(
    IOrganisationClient organisationClient,
    ISession session) : LoggedInUserAwareModel(session)
{
    [BindProperty]
    public IList<ApprovableOrganisation> BuyerOrganisations { get; set; } = [];

    [BindProperty]
    public IList<ApprovableOrganisation> SupplierOrganisations { get; set; } = [];

    public async Task<IActionResult> OnGet()
    {
        // Pagination will be put in place in a later update
        BuyerOrganisations = (await organisationClient.GetAllOrganisationsAsync("buyer", 1000, 0)).ToList();

        SupplierOrganisations = (await organisationClient.GetAllOrganisationsAsync("supplier", 1000, 0)).ToList();

        return Page();
    }
}