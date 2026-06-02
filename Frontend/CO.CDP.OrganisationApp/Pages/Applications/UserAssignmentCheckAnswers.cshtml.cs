using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace CO.CDP.OrganisationApp.Pages.Applications;

[Authorize(Policy = OrgScopeRequirement.Admin)]
public class UserAssignmentCheckAnswersModel(
    IOrganisationClient organisationClient,
    IAppRegistryClient appRegistryClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid AppId { get; set; }

    public UserAssignmentState? State { get; private set; }

    public async Task<IActionResult> OnGet()
    {
        if (!await GuardBuyerAsync()) return Redirect("/page-not-found");

        State = ReadState();
        if (State == null) return Redirect($"/organisation/{Id}/applications/{AppId}/user-assignments/assign");

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!await GuardBuyerAsync()) return Redirect("/page-not-found");

        State = ReadState();
        if (State == null) return Redirect($"/organisation/{Id}/applications/{AppId}/user-assignments/assign");

        try
        {
            if (State.IsEdit)
            {
                await appRegistryClient.UpdateUserRolesAsync(
                    Id, AppId, State.UserPrincipalId,
                    new UpdateAppRegistryUserAssignment(State.SelectedRoleIds));
            }
            else
            {
                await appRegistryClient.AssignUserAsync(
                    Id, AppId,
                    new CreateAppRegistryUserAssignment(State.UserPrincipalId, State.SelectedRoleIds));
            }
        }
        catch (HttpRequestException ex)
        {
            ModelState.AddModelError(string.Empty, $"Could not save the assignment: {ex.Message}");
            return Page();
        }

        TempData.Remove(UserAssignmentState.TempDataKey);
        return Redirect($"/organisation/{Id}/applications/{AppId}/user-assignments");
    }

    private UserAssignmentState? ReadState()
    {
        var json = TempData.Peek(UserAssignmentState.TempDataKey) as string;
        return json == null ? null : JsonSerializer.Deserialize<UserAssignmentState>(json);
    }

    private async Task<bool> GuardBuyerAsync()
    {
        try
        {
            var org = await organisationClient.GetOrganisationAsync(Id);
            return org.IsBuyer();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return false;
        }
    }
}
