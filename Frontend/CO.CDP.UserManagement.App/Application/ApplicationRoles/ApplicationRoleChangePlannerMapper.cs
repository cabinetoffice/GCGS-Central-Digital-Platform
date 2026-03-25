using CO.CDP.UserManagement.Core.ApplicationRoles;
using CO.CDP.UserManagement.App.Models;

namespace CO.CDP.UserManagement.App.Application.ApplicationRoles;

public static class ApplicationRoleChangePlannerMapper
{
    public static ApplicationRoleChangeInput Map(
        ChangeUserApplicationRolesViewModel vm,
        ApplicationRoleChangePostModel posted,
        string organisationSlug,
        Guid? cdpPersonId,
        Guid? inviteGuid)
    {
        var postedMap = posted.Applications.ToDictionary(a => a.OrganisationApplicationId);

        return new ApplicationRoleChangeInput(
            OrganisationSlug: organisationSlug,
            CdpPersonId: cdpPersonId,
            InviteGuid: inviteGuid,
            UserDisplayName: vm.UserDisplayName,
            Email: vm.Email,
            Applications: vm.Applications.Select(app =>
            {
                postedMap.TryGetValue(app.OrganisationApplicationId, out var post);
                return new ApplicationRoleChangeInputItem(
                    OrganisationApplicationId: app.OrganisationApplicationId,
                    ApplicationId: app.ApplicationId,
                    ApplicationName: app.ApplicationName,
                    HasExistingAccess: app.HasExistingAccess,
                    GiveAccess: app.HasExistingAccess && (post?.GiveAccess ?? false),
                    OriginalSingleRoleId: app.SelectedRoleId,
                    OriginalMultiRoleIds: app.SelectedRoleIds.ToList(),
                    SelectedRoleId: post?.SelectedRoleId,
                    SelectedRoleIds: post?.SelectedRoleIds ?? [],
                    AllowsMultipleRoleAssignments: app.AllowsMultipleRoleAssignments,
                    AvailableRoles: app.Roles.Select(r => new ApplicationRoleOption(r.Id, r.Name)).ToList()
                );
            }).ToList()
        );
    }
}