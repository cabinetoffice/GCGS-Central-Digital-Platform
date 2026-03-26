using CO.CDP.UserManagement.App.Models;
using CO.CDP.Functional;

namespace CO.CDP.UserManagement.App.Application.Users
{
    public interface IUsersQueryService
    {
        Task<UsersViewModel?> GetViewModelAsync(
            string? organisationSlug,
            string? role,
            string? application,
            string? search,
            CancellationToken ct);
    }

    public interface IUserDetailsQueryService
    {
        Task<UserDetailsViewModel?> GetViewModelAsync(
            string organisationSlug,
            Guid cdpPersonId,
            CancellationToken ct);

        Task<RemoveApplicationSuccessViewModel?> GetRemoveApplicationSuccessViewModelAsync(
            string organisationSlug,
            Guid cdpPersonId,
            string clientId,
            CancellationToken ct);
    }

    public interface IInviteDetailsQueryService
    {
        Task<InviteDetailsViewModel?> GetViewModelAsync(
            string organisationSlug,
            Guid inviteGuid,
            CancellationToken ct);
    }
}
