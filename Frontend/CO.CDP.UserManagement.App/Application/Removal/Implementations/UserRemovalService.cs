using System;
using System.Threading;
using System.Threading.Tasks;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Adapters;
using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.App.Application.Removal.Implementations
{
    public class UserRemovalService : IUserRemovalService
    {
        private readonly IUserManagementApiAdapter _adapter;
        public UserRemovalService(IUserManagementApiAdapter adapter)
        {
            _adapter = adapter;
        }

        public async Task<RemoveUserViewModel?> GetUserViewModelAsync(string organisationSlug, Guid cdpPersonId, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationBySlugAsync(organisationSlug, ct);
            if (org is null) return null;

            var user = await _adapter.GetUserAsync(org.CdpOrganisationGuid, cdpPersonId, ct);
            if (user is null) return null;

            return new RemoveUserViewModel(
                OrganisationName: org.Name,
                OrganisationSlug: organisationSlug,
                UserDisplayName: $"{user.FirstName} {user.LastName}",
                Email: user.Email,
                CurrentRole: user.OrganisationRole,
                MemberSinceFormatted: user.JoinedAt?.ToString("MMMM dd, yyyy") ?? string.Empty,
                CdpPersonId: cdpPersonId,
                PendingInviteId: null,
                RemoveConfirmed: null);
            
        }

        public async Task<RemoveUserViewModel?> GetInviteViewModelAsync(string organisationSlug, int pendingInviteId, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationBySlugAsync(organisationSlug, ct);
            if (org is null) return null;

            var invites = await _adapter.GetInvitesAsync(org.CdpOrganisationGuid, ct);
            var invite = invites.FirstOrDefault(i => i.PendingInviteId == pendingInviteId);
            if (invite is null) return null;

            return new RemoveUserViewModel(
                OrganisationName: org.Name,
                OrganisationSlug: organisationSlug,
                UserDisplayName: $"{invite.FirstName} {invite.LastName}",
                Email: invite.Email,
                CurrentRole: invite.OrganisationRole,
                MemberSinceFormatted: string.Empty,
                CdpPersonId: null,
                PendingInviteId: pendingInviteId,
                RemoveConfirmed: null);
            
        }

        public async Task<bool> IsLastOwnerAsync(
            string organisationSlug, Guid cdpPersonId, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationBySlugAsync(organisationSlug, ct);
            if (org is null) return false;

            var users = await _adapter.GetUsersAsync(org.CdpOrganisationGuid, ct);
            var owners = users.Where(u => u.OrganisationRole == OrganisationRole.Owner).ToList();

            return owners.Count == 1 &&
                   owners[0].CdpPersonId == cdpPersonId;
        }

        public async Task<Result<ServiceFailure, ServiceOutcome>> RemoveUserAsync(string organisationSlug, Guid cdpPersonId, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationBySlugAsync(organisationSlug, ct);
            if (org is null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

            return await _adapter.RemoveUserAsync(org.CdpOrganisationGuid, cdpPersonId, ct);
        }

        public async Task<Result<ServiceFailure, ServiceOutcome>> RemoveInviteAsync(string organisationSlug, int pendingInviteId, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationBySlugAsync(organisationSlug, ct);
            if (org is null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

            return await _adapter.CancelInviteAsync(org.CdpOrganisationGuid, pendingInviteId, ct);
        }
    }
}
