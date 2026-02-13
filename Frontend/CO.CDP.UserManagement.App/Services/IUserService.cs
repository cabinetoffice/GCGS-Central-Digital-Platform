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
        CancellationToken ct = default);

    Task<ChangeUserRoleViewModel?> GetChangeUserRoleViewModelAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        int? pendingInviteId,
        CancellationToken ct = default);

    Task<bool> UpdateUserRoleAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        int? pendingInviteId,
        OrganisationRole organisationRole,
        CancellationToken ct = default);

    Task<bool> ResendInviteAsync(
        string organisationSlug,
        int pendingInviteId,
        CancellationToken ct = default);
}
