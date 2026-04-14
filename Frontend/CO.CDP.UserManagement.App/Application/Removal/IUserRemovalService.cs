using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.Core.Removal;

namespace CO.CDP.UserManagement.App.Application.Removal
{
    public interface IUserRemovalService
    {
        Task<RemoveUserViewModel?> GetUserViewModelAsync(
            string organisationSlug,
            Guid cdpPersonId,
            CancellationToken ct);

        Task<RemoveUserViewModel?> GetInviteViewModelAsync(
            string organisationSlug,
            int pendingInviteId,
            CancellationToken ct);

        Task<RemoveApplicationViewModel?> GetRemoveApplicationViewModelAsync(
            string organisationSlug, Guid cdpPersonId, string clientId, CancellationToken ct);

        Task<RemoveSuccessViewModel?> GetRemoveSuccessViewModelAsync(
            string organisationSlug, Guid cdpPersonId, CancellationToken ct);

        Task<RemovalValidationResult> ValidateRemovalAsync(
            string organisationSlug,
            Guid cdpPersonId,
            CancellationToken ct);

        Task<UserRemovalSubmitResult> ValidateAndRemoveUserAsync(
            string organisationSlug,
            Guid cdpPersonId,
            bool? removeConfirmed,
            CancellationToken ct);

        Task<UserRemovalSubmitResult> RemoveUserAsync(
            string organisationSlug,
            Guid cdpPersonId,
            CancellationToken ct);

        Task<InviteRemovalSubmitResult> RemoveInviteAsync(
            string organisationSlug,
            int pendingInviteId,
            CancellationToken ct);

        Task<ApplicationRemovalSubmitResult> RemoveApplicationAsync(
            string organisationSlug, Guid cdpPersonId, string clientId, CancellationToken ct);
    }
}
