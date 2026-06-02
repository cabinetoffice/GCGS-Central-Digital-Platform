using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Applications;

[Authorize(Policy = OrgScopeRequirement.Admin)]
public class UserAssignmentsModel(
    IOrganisationClient organisationClient,
    IAppRegistryClient appRegistryClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid AppId { get; set; }

    public string ApplicationName { get; set; } = string.Empty;
    public string OrganisationName { get; set; } = string.Empty;
    public ICollection<AppRegistryUserAssignmentDto> Assignments { get; set; } = [];

    public async Task<IActionResult> OnGet()
    {
        CO.CDP.Organisation.WebApiClient.Organisation? org;
        try { org = await organisationClient.GetOrganisationAsync(Id); }
        catch (ApiException ex) when (ex.StatusCode == 404) { return Redirect("/page-not-found"); }
        if (!org.IsBuyer()) return Redirect("/page-not-found");

        OrganisationName = org.Name;

        var app = await appRegistryClient.GetApplicationAsync(AppId);
        if (app == null) return Redirect("/page-not-found");
        ApplicationName = app.Name;

        Assignments = await appRegistryClient.GetUserAssignmentsAsync(Id, AppId);
        return Page();
    }
}
