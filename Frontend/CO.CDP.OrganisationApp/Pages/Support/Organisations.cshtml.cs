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
        var userDetails = SessionContext.Get<UserDetails>(Session.UserDetailsKey);

        // var organisations = await organisationClient.GetAllOrganisationsAsync(50, 0);

        BuyerOrganisations = (await organisationClient.GetAllOrganisationsAsync("buyer", 50, 0)).ToList();

        SupplierOrganisations = (await organisationClient.GetAllOrganisationsAsync("supplier", 50, 0)).ToList();

        // organisations.ToList()
        //     .ForEach(o =>
        //     {
        //         if (o.Role == "buyer")
        //         {
        //             BuyerOrganisations.Add(o);
        //         }
        //         else if (o.Role == "supplier")
        //         {
        //             SupplierOrganisations.Add(o);
        //         }
        //     });

        return Page();
    }
}