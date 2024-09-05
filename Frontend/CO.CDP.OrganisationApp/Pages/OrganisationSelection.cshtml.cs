using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Pages;

public class OrganisationSelectionModel(
    IOrganisationClient organisationClient,
    ISession session) : LoggedInUserAwareModel(session)
{
    public IEnumerable<Organisation.WebApiClient.Organisation> Organisations { get; set; } = [];

    public string? Error { get; set; }

    public async Task<IActionResult> OnGet()
    {
        Organisations = await organisationClient.ListOrganisationsAsync(UserDetails.UserUrn);

        return Page();
    }

    public IActionResult OnPost()
    {
        return RedirectToPage("/Registration/OrganisationType");
    }
}