using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.App.Services;

public interface IUserService
{
    Task<UsersViewModel?> GetUsersViewModelAsync(
        string organisationSlug,
        string? selectedRole = null,
        string? selectedApplication = null,
        string? searchTerm = null,
        CancellationToken ct = default);

    Task<InviteUserViewModel?> GetInviteUserViewModelAsync(
        string organisationSlug,
        InviteUserViewModel? input = null,
        CancellationToken ct = default);

    Task<bool> InviteUserAsync(
        string organisationSlug,
        InviteUserViewModel input,
        CancellationToken ct = default,
        IReadOnlyList<InviteApplicationAssignment>? applicationAssignments = null);

    Task<ApplicationRolesStepViewModel?> GetApplicationRolesStepViewModelAsync(
        string organisationSlug,
        InviteUserState state,
        CancellationToken ct = default);

    Task<ChangeUserRoleViewModel?> GetChangeUserRoleViewModelAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        CancellationToken ct = default);

    Task<bool> UpdateUserRoleAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        OrganisationRole organisationRole,
        CancellationToken ct = default);

    Task<bool> ResendInviteAsync(
        string organisationSlug,
        Guid inviteGuid,
        CancellationToken ct = default);

    Task<ChangeUserApplicationRolesViewModel?> GetChangeUserApplicationRolesViewModelAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        CancellationToken ct = default);

    Task<bool> UpdateUserApplicationRolesAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        IReadOnlyList<ApplicationRoleAssignmentPostModel> applicationRoleAssignments,
        CancellationToken ct = default);
}
