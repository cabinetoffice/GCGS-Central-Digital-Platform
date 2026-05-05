using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.Core.Removal;

namespace CO.CDP.UserManagement.App.Application.Removal;

public interface IUserRemovalService
{
    Task<RemoveUserViewModel?> GetUserViewModelAsync(
        Guid id,
        Guid cdpPersonId,
        CancellationToken ct);

    Task<RemoveUserViewModel?> GetInviteViewModelAsync(
        Guid id,
        Guid inviteGuid,
        CancellationToken ct);

    Task<RemoveApplicationViewModel?> GetRemoveApplicationViewModelAsync(
        Guid id, Guid cdpPersonId, string clientId, CancellationToken ct);

    Task<RemoveSuccessViewModel?> GetRemoveSuccessViewModelAsync(
        Guid id, Guid cdpPersonId, CancellationToken ct);

    Task<RemovalValidationResult> ValidateRemovalAsync(
        Guid id,
        Guid cdpPersonId,
        CancellationToken ct);

    Task<UserRemovalSubmitResult> ValidateAndRemoveUserAsync(
        Guid id,
        Guid cdpPersonId,
        bool? removeConfirmed,
        CancellationToken ct);

    Task<InviteRemovalSubmitResult> RemoveInviteAsync(
        Guid id,
        Guid inviteGuid,
        CancellationToken ct);

    Task<ApplicationRemovalSubmitResult> RemoveApplicationAsync(
        Guid id, Guid cdpPersonId, string clientId, CancellationToken ct);
}
