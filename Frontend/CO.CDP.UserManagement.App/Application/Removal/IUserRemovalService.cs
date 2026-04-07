using System;
using System.Threading;
using System.Threading.Tasks;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Services;

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

        Task<Result<ServiceFailure, ServiceOutcome>> RemoveApplicationAsync(
            string organisationSlug, Guid cdpPersonId, string clientId, CancellationToken ct);

        Task<bool> IsLastOwnerAsync(
            string organisationSlug,
            Guid cdpPersonId,
            CancellationToken ct);

        Task<Result<ServiceFailure, ServiceOutcome>> RemoveUserAsync(
            string organisationSlug,
            Guid cdpPersonId,
            CancellationToken ct);

        Task<Result<ServiceFailure, ServiceOutcome>> RemoveInviteAsync(
            string organisationSlug,
            int pendingInviteId,
            CancellationToken ct);

        Task<UserRemovalSubmitResult> ValidateAndRemoveUserAsync(
            string organisationSlug,
            Guid cdpPersonId,
            bool? removeConfirmed,
            CancellationToken ct);
    }
}
