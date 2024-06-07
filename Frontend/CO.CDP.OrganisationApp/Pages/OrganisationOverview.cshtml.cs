using CO.CDP.Organisation.WebApiClient;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages;

[AuthorisedSession]
public class OrganisationOverviewModel(
    IOrganisationClient organisationClient,
    ISession session) : LoggedInUserAwareModel
{
    public override ISession SessionContext => session;

    public OrganisationWebApiClient.Organisation? OrganisationDetails { get; set; }

    public async Task OnGet(Guid? id)
    {
        ArgumentNullException.ThrowIfNull(id);

        OrganisationDetails = await organisationClient.GetOrganisationAsync(id.Value);
    }
}