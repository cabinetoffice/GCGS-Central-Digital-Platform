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
    public IList<Organisation.WebApiClient.Organisation> BuyerOrganisations { get; set; } = [];

    [BindProperty]
    public IList<Organisation.WebApiClient.Organisation> SupplierOrganisations { get; set; } = [];

    public async Task<IActionResult> OnGet()
    {
        var userDetails = SessionContext.Get<UserDetails>(Session.UserDetailsKey);

        var organisations = await organisationClient.ListOrganisationsAsync(userDetails.UserUrn);

        organisations.ToList()
            .ForEach(o =>
            {
                if (o.Roles.Contains(PartyRole.Supplier))
                {
                    SupplierOrganisations.Add(o);
                }
                else
                {
                    BuyerOrganisations.Add(o);
                }
            });

        return Page();
    }
}