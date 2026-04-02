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

        public async Task<RemoveUserViewModel?> GetInviteViewModelAsync(string organisationSlug, Guid inviteGuid, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationBySlugAsync(organisationSlug, ct);
            if (org is null) return null;

            var invite = await _adapter.GetInviteAsync(org.CdpOrganisationGuid, inviteGuid, ct);
            if (invite is null) return null;

            return new RemoveUserViewModel(
                OrganisationName: org.Name,
                OrganisationSlug: organisationSlug,
                UserDisplayName: $"{invite.FirstName} {invite.LastName}",
                Email: invite.Email,
                CurrentRole: invite.OrganisationRole,
                MemberSinceFormatted: invite.CreatedAt.ToString("d MMMM yyyy"),
                CdpPersonId: null,
                PendingInviteId: invite.PendingInviteId,
                RemoveConfirmed: null);

        }

        public async Task<RemoveApplicationViewModel?> GetRemoveApplicationViewModelAsync(string organisationSlug, Guid cdpPersonId, string clientId, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationBySlugAsync(organisationSlug, ct);
            if (org is null) return null;

            var user = await _adapter.GetUserAsync(org.CdpOrganisationGuid, cdpPersonId, ct);
            if (user is null) return null;

            var apps = await _adapter.GetApplicationsAsync(org.Id, ct);
            var app = apps.FirstOrDefault(a => a.Application?.ClientId == clientId);
            if (app is null || app.Application is null) return null;

            var assignments = await _adapter.GetUserAssignmentsAsync(org.Id, cdpPersonId, ct);
            var assignment = assignments.FirstOrDefault(a => a.IsActive && a.OrganisationApplicationId == app.Id);

            return new RemoveApplicationViewModel(
                OrganisationSlug: organisationSlug,
                UserDisplayName: $"{user.FirstName} {user.LastName}",
                UserEmail: user.Email,
                ApplicationName: app.Application.Name,
                ApplicationSlug: app.Application.ClientId,
                AssignmentId: assignment?.Id ?? 0,
                OrgId: org.Id,
                UserPrincipalId: cdpPersonId.ToString(),
                RoleName: assignment?.Roles?.FirstOrDefault()?.Name ?? string.Empty,
                AssignedAt: assignment?.AssignedAt ?? assignment?.CreatedAt,
                AssignedByName: assignment?.AssignedBy,
                CdpPersonId: cdpPersonId,
                RevokeConfirmed: null);
        }

        public async Task<RemoveSuccessViewModel?> GetRemoveSuccessViewModelAsync(string organisationSlug, Guid cdpPersonId, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationBySlugAsync(organisationSlug, ct);
            if (org is null) return null;

            var user = await _adapter.GetUserAsync(org.CdpOrganisationGuid, cdpPersonId, ct);
            if (user is null) return null;

            return new RemoveSuccessViewModel
            {
                OrganisationSlug = organisationSlug,
                UserDisplayName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                OrganisationName = org.Name,
                MemberSince = user.JoinedAt?.ToString("dd MMMM yyyy") ?? string.Empty,
                Role = user.OrganisationRole,
                CdpPersonId = cdpPersonId
            };
        }

        public async Task<Result<ServiceFailure, ServiceOutcome>> RemoveApplicationAsync(string organisationSlug, Guid cdpPersonId, string clientId, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationBySlugAsync(organisationSlug, ct);
            if (org is null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

            var apps = await _adapter.GetApplicationsAsync(org.Id, ct);
            var app = apps.FirstOrDefault(a => a.Application?.ClientId == clientId);
            if (app is null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

            var assignments = await _adapter.GetUserAssignmentsAsync(org.Id, cdpPersonId, ct);
            var assignment = assignments.FirstOrDefault(a => a.IsActive && a.OrganisationApplicationId == app.Id);
            if (assignment is null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

            return await _adapter.DeleteUserAssignmentAsync(org.Id, cdpPersonId, assignment.Id, ct);
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

        public async Task<Result<ServiceFailure, ServiceOutcome>> RemoveInviteAsync(string organisationSlug, Guid inviteGuid, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationBySlugAsync(organisationSlug, ct);
            if (org is null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

            var invite = await _adapter.GetInviteAsync(org.CdpOrganisationGuid, inviteGuid, ct);
            if (invite is null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

            return await _adapter.CancelInviteAsync(org.CdpOrganisationGuid, invite.PendingInviteId, ct);
        }
    }
}
