using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Applications;

[Authorize(Policy = OrgScopeRequirement.Admin)]
public class RevokeConfirmationModel(
    IOrganisationClient organisationClient,
    IAppRegistryClient appRegistryClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid AppId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string UserId { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Select yes if you want to remove this user's access")]
    public bool? ConfirmRevoke { get; set; }

    public string ApplicationName { get; set; } = string.Empty;
    public string OrganisationName { get; set; } = string.Empty;
    public IList<string> CurrentRoles { get; set; } = [];

    public async Task<IActionResult> OnGet()
    {
        var loaded = await LoadPageDataAsync();
        return loaded ?? Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            await LoadPageDataAsync();
            return Page();
        }

        if (ConfirmRevoke == false)
        {
            return Redirect($"/organisation/{Id}/applications/{AppId}/user-assignments");
        }

        try
        {
            await appRegistryClient.RevokeUserAsync(Id, AppId, Uri.UnescapeDataString(UserId));
        }
        catch (HttpRequestException ex)
        {
            ModelState.AddModelError(string.Empty, $"Could not revoke access: {ex.Message}");
            await LoadPageDataAsync();
            return Page();
        }

        return Redirect($"/organisation/{Id}/applications/{AppId}/user-assignments");
    }

    private async Task<IActionResult?> LoadPageDataAsync()
    {
        CO.CDP.Organisation.WebApiClient.Organisation? org;
        try { org = await organisationClient.GetOrganisationAsync(Id); }
        catch (ApiException ex) when (ex.StatusCode == 404) { return Redirect("/page-not-found"); }
        if (!org.IsBuyer()) return Redirect("/page-not-found");

        OrganisationName = org.Name;

        var app = await appRegistryClient.GetApplicationAsync(AppId);
        if (app == null) return Redirect("/page-not-found");
        ApplicationName = app.Name;

        var decodedUserId = Uri.UnescapeDataString(UserId);
        var assignments   = await appRegistryClient.GetUserAssignmentsAsync(Id, AppId);
        var assignment    = assignments.FirstOrDefault(a => a.UserPrincipalId == decodedUserId);
        if (assignment == null) return Redirect("/page-not-found");

        CurrentRoles = assignment.Roles.Select(r => r.Name).ToList();
        return null;
    }
}
