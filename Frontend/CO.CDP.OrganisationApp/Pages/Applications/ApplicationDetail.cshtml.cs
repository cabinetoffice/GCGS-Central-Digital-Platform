using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Applications;

[Authorize(Policy = OrgScopeRequirement.Admin)]
public class ApplicationDetailModel(
    IOrganisationClient organisationClient,
    IAppRegistryClient appRegistryClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid AppId { get; set; }

    public AppRegistryApplicationDto? Application { get; set; }
    public ICollection<AppRegistryRoleDto> Roles { get; set; } = [];

    public async Task<IActionResult> OnGet()
    {
        CO.CDP.Organisation.WebApiClient.Organisation? org;
        try
        {
            org = await organisationClient.GetOrganisationAsync(Id);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        if (!org.IsBuyer()) return Redirect("/page-not-found");

        var appTask   = appRegistryClient.GetApplicationAsync(AppId);
        var rolesTask = appRegistryClient.GetApplicationRolesAsync(AppId);

        await Task.WhenAll(appTask, rolesTask);

        Application = appTask.Result;
        Roles       = rolesTask.Result;

        if (Application == null) return Redirect("/page-not-found");

        return Page();
    }
}
