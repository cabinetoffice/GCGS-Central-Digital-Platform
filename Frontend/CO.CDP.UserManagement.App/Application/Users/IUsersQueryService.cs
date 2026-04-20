using CO.CDP.UserManagement.App.Models;
using CO.CDP.Functional;

namespace CO.CDP.UserManagement.App.Application.Users
{
    public interface IUsersQueryService
    {
        Task<UsersViewModel?> GetViewModelAsync(
            Guid id,
            string? role,
            string? application,
            string? search,
            CancellationToken ct);
    }

    public interface IUserDetailsQueryService
    {
        Task<UserDetailsViewModel?> GetViewModelAsync(
            Guid id,
            Guid cdpPersonId,
            CancellationToken ct);

        Task<RemoveApplicationSuccessViewModel?> GetRemoveApplicationSuccessViewModelAsync(
            Guid id,
            Guid cdpPersonId,
            string clientId,
            CancellationToken ct);
    }

    public interface IInviteDetailsQueryService
    {
        Task<InviteDetailsViewModel?> GetViewModelAsync(
            Guid id,
            Guid inviteGuid,
            CancellationToken ct);
    }
}
