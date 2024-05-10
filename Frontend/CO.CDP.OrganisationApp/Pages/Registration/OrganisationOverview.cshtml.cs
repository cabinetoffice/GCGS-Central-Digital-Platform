using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
public class OrganisationOverviewModel(
    IOrganisationClient organisationClient) : PageModel
{

    public OrganisationWebApiClient.Organisation? OrganisationDetails { get; set; }

    public async Task OnGet(Guid? id)
    {
        ArgumentNullException.ThrowIfNull(id);

        OrganisationDetails = await organisationClient.GetOrganisationAsync(id!.Value);
    }
}