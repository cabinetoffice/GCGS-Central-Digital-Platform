using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages;

public class OrganisationOverviewModel(IOrganisationClient organisationClient) : PageModel
{
    public OrganisationWebApiClient.Organisation? OrganisationDetails { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            OrganisationDetails = await organisationClient.GetOrganisationAsync(Id);
            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }
}