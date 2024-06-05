using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages;

[Authorize]
public class OrganisationSelectionModel(
    IOrganisationClient organisationClient,
    ISession session) : PageModel
{
    public IEnumerable<Organisation.WebApiClient.Organisation> Organisations { get; set; } = [];

    public string? Error { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var details = session.Get<UserDetails>(Session.UserDetailsKey);
        if (details == null)
        {
            return Redirect("/one-login/user-info");
        }

        Organisations = await organisationClient.ListOrganisationsAsync(details.UserUrn);

        return Page();
    }

    public IActionResult OnPost()
    {
        return RedirectToPage("/Registration/OrganisationType");
    }
}