using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.App.Models;

public sealed record ChangeUserRoleViewModel(
    string OrganisationName,
    string OrganisationSlug,
    string UserDisplayName,
    string Email,
    OrganisationRole CurrentRole,
    OrganisationRole? SelectedRole,
    bool IsPending,
    Guid? CdpPersonId,
    Guid? InviteGuid)
{
    public static ChangeUserRoleViewModel Empty => new(
        OrganisationName: string.Empty,
        OrganisationSlug: string.Empty,
        UserDisplayName: string.Empty,
        Email: string.Empty,
        CurrentRole: OrganisationRole.Member,
        SelectedRole: null,
        IsPending: false,
        CdpPersonId: null,
        InviteGuid: null);
}

public sealed record ChangeUserRoleSuccessViewModel(
    string OrganisationSlug,
    string UserDisplayName,
    OrganisationRole NewRole,
    string RoleDescription);
