using CO.CDP.UserManagement.App.Models;

namespace CO.CDP.UserManagement.App.Application.ApplicationRoles;

public static class ApplicationRoleAssignmentsFactory
{
    public static IReadOnlyList<ApplicationRoleAssignmentPostModel> Build(
        ChangeApplicationRoleState state)
    {
        return state.Applications
            .Where(a => a.GiveAccess &&
                        (a.SelectedRoleIds is { Count: > 0 } || a.SelectedRoleId.HasValue))
            .Select(a => new ApplicationRoleAssignmentPostModel
            {
                OrganisationApplicationId = a.OrganisationApplicationId,
                ApplicationId = a.ApplicationId,
                GiveAccess = a.GiveAccess,
                SelectedRoleId = a.SelectedRoleId,
                SelectedRoleIds = a.SelectedRoleIds?.ToList() ?? []
            })
            .ToList();
    }
}