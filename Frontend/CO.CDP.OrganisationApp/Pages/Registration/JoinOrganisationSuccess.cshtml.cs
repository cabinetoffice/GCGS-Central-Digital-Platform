using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Registration;

public class JoinOrganisationSuccessModel(
    IOrganisationClient organisationClient) : PageModel
{
    public OrganisationWebApiClient.Organisation? OrganisationDetails { get; set; }

    public async Task<IActionResult> OnGet(string identifier)
    {
        try
        {
            OrganisationDetails = await organisationClient.LookupOrganisationAsync(string.Empty, $"{identifier}");
            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }
}