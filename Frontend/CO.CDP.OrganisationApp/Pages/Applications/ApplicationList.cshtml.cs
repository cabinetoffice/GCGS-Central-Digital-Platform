using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Applications;

[Authorize(Policy = OrgScopeRequirement.Admin)]
public class ApplicationListModel(
    IOrganisationClient organisationClient,
    IAppRegistryClient appRegistryClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public ICollection<AppRegistryApplicationDto> Applications { get; set; } = [];

    public async Task<IActionResult> OnGet()
    {
        // Buyer guard — this page is only accessible to Buyer organisations.
        CO.CDP.Organisation.WebApiClient.Organisation? org;
        try
        {
            org = await organisationClient.GetOrganisationAsync(Id);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        if (!org.IsBuyer())
        {
            return Redirect("/page-not-found");
        }

        try
        {
            Applications = await appRegistryClient.GetOrganisationApplicationsAsync(Id);
        }
        catch (HttpRequestException)
        {
            // If AppRegistry returns an error, show empty list rather than crashing.
            Applications = [];
        }

        return Page();
    }
}
