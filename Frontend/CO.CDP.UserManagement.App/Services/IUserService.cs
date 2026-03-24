using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.Functional;

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

    Task<Result<ServiceFailure, ServiceOutcome>> InviteUserAsync(
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

    Task<Result<ServiceFailure, ServiceOutcome>> UpdateUserRoleAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        OrganisationRole organisationRole,
        CancellationToken ct = default);

    Task<Result<ServiceFailure, ServiceOutcome>> ResendInviteAsync(
        string organisationSlug,
        Guid inviteGuid,
        CancellationToken ct = default);

    Task<ChangeUserApplicationRolesViewModel?> GetChangeUserApplicationRolesViewModelAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        CancellationToken ct = default);

    Task<Result<ServiceFailure, ServiceOutcome>> UpdateUserApplicationRolesAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        IReadOnlyList<ApplicationRoleAssignmentPostModel> applicationRoleAssignments,
        CancellationToken ct = default);

    Task<RemoveUserViewModel?> GetRemoveUserViewModelAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        int? pendingInviteId,
        CancellationToken ct = default);

    Task<bool> RemoveUserAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        int? pendingInviteId,
        CancellationToken ct = default);

    Task<RemoveApplicationSuccessViewModel?> GetRemoveApplicationSuccessViewModelAsync(
        string organisationSlug,
        Guid cdpPersonId,
        string clientId,
        CancellationToken ct = default);

    Task<UserDetailsViewModel?> GetUserDetailsViewModelAsync(
        string organisationSlug,
        Guid cdpPersonId,
        CancellationToken ct = default);
}
