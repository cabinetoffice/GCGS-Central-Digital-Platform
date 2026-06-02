using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace CO.CDP.OrganisationApp.Pages.Applications;

[Authorize(Policy = OrgScopeRequirement.Admin)]
public class AssignUserModel(
    IOrganisationClient organisationClient,
    IAppRegistryClient appRegistryClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid AppId { get; set; }

    /// <summary>True when editing an existing assignment (pre-populated from edit-roles route).</summary>
    [BindProperty(SupportsGet = true)]
    public bool IsEdit { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select a member")]
    public string? SelectedUserPrincipalId { get; set; }

    [BindProperty]
    [MinLength(1, ErrorMessage = "Select at least one role")]
    public IList<Guid> SelectedRoleIds { get; set; } = [];

    public string ApplicationName { get; set; } = string.Empty;
    public string OrganisationName { get; set; } = string.Empty;
    public ICollection<AppRegistryMemberDto> AvailableMembers { get; set; } = [];
    public ICollection<AppRegistryRoleDto> AvailableRoles { get; set; } = [];

    public async Task<IActionResult> OnGet(string? userId = null)
    {
        var loaded = await LoadPageDataAsync();
        if (loaded is not null) return loaded;

        // Pre-populate when editing
        if (IsEdit && userId != null)
        {
            SelectedUserPrincipalId = Uri.UnescapeDataString(userId);
            var existing = (await appRegistryClient.GetUserAssignmentsAsync(Id, AppId))
                .FirstOrDefault(a => a.UserPrincipalId == SelectedUserPrincipalId);
            if (existing != null)
                SelectedRoleIds = existing.Roles.Select(r => r.Id).ToList();
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            await LoadPageDataAsync();
            return Page();
        }

        // Build and save state for check-answers
        var role = AvailableRoles.Count == 0
            ? (await appRegistryClient.GetApplicationRolesAsync(AppId))
            : AvailableRoles;

        var selectedRoles = role.Where(r => SelectedRoleIds.Contains(r.Id)).ToList();

        var state = new UserAssignmentState
        {
            ApplicationId      = AppId,
            ApplicationName    = ApplicationName,
            UserPrincipalId    = SelectedUserPrincipalId!,
            UserDisplayName    = SelectedUserPrincipalId!,
            SelectedRoleIds    = SelectedRoleIds,
            SelectedRoleNames  = selectedRoles.Select(r => r.Name).ToList(),
            IsEdit             = IsEdit
        };

        TempData[UserAssignmentState.TempDataKey] = JsonSerializer.Serialize(state);

        return Redirect($"/organisation/{Id}/applications/{AppId}/user-assignments/check-answers");
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

        var rolesTask       = appRegistryClient.GetApplicationRolesAsync(AppId);
        var membersTask     = appRegistryClient.GetOrganisationMembersAsync(Id);
        var assignmentsTask = appRegistryClient.GetUserAssignmentsAsync(Id, AppId);

        await Task.WhenAll(rolesTask, membersTask, assignmentsTask);

        AvailableRoles = rolesTask.Result;

        // Filter out members already assigned (unless editing)
        var assignedUrns = assignmentsTask.Result.Select(a => a.UserPrincipalId).ToHashSet();
        AvailableMembers = membersTask.Result
            .Where(m => m.IsActive && (IsEdit || !assignedUrns.Contains(m.UserPrincipalId)))
            .ToList();

        return null;
    }
}
