using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Users;

[Authorize(Policy = OrgScopeRequirement.Viewer)]
public class ViewAdminsModel(
    IOrganisationClient organisationClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public ICollection<CO.CDP.Organisation.WebApiClient.Person> Persons { get; set; } = [];

    public async Task<IActionResult> OnGet()
    {
        Persons = await organisationClient.GetOrganisationPersonsInRoleAsync(Id, OrganisationPersonScopes.Admin);

        return Page();
    }
}