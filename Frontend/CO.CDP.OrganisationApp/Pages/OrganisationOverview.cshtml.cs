using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages;

[AuthorisedSession]
public class OrganisationOverviewModel(
    IOrganisationClient organisationClient,
    ISession session) : LoggedInUserAwareModel
{
    public override ISession SessionContext => session;

    public OrganisationWebApiClient.Organisation? OrganisationDetails { get; set; }

    public async Task<IActionResult> OnGet(Guid id)
    {
        try
        {
            OrganisationDetails = await organisationClient.GetOrganisationAsync(id);
            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }
}