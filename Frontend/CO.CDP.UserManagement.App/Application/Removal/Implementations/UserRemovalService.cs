using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Adapters;
using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.Removal;
using CO.CDP.UserManagement.Shared.Responses;

namespace CO.CDP.UserManagement.App.Application.Removal.Implementations
{
    public class UserRemovalService : IUserRemovalService
    {
        private readonly IUserManagementApiAdapter _adapter;
        private readonly ICurrentUserService _currentUserService;

        public UserRemovalService(IUserManagementApiAdapter adapter, ICurrentUserService currentUserService)
        {
            _adapter = adapter;
            _currentUserService = currentUserService;
        }

        public async Task<RemoveUserViewModel?> GetUserViewModelAsync(Guid id, Guid cdpPersonId, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationByGuidAsync(id, ct);
            if (org is null) return null;

            var user = await _adapter.GetUserAsync(org.CdpOrganisationGuid, cdpPersonId, ct);
            if (user is null) return null;

            return new RemoveUserViewModel(
                OrganisationName: org.Name,
                OrganisationId: id,
                UserDisplayName: $"{user.FirstName} {user.LastName}",
                Email: user.Email,
                CurrentRole: user.OrganisationRole,
                MemberSinceFormatted: user.JoinedAt?.ToString("MMMM dd, yyyy") ?? string.Empty,
                CdpPersonId: cdpPersonId,
                PendingInviteId: null,
                RemoveConfirmed: null);
        }

        public async Task<RemoveUserViewModel?> GetInviteViewModelAsync(Guid id, Guid inviteGuid, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationByGuidAsync(id, ct);
            if (org is null) return null;

            var invite = await _adapter.GetInviteAsync(org.CdpOrganisationGuid, inviteGuid, ct);
            if (invite is null) return null;

            return new RemoveUserViewModel(
                OrganisationName: org.Name,
                OrganisationId: id,
                UserDisplayName: $"{invite.FirstName} {invite.LastName}",
                Email: invite.Email,
                CurrentRole: invite.OrganisationRole,
                MemberSinceFormatted: invite.CreatedAt.ToString("d MMMM yyyy"),
                CdpPersonId: null,
                PendingInviteId: invite.PendingInviteId,
                RemoveConfirmed: null);
        }

        public async Task<RemoveApplicationViewModel?> GetRemoveApplicationViewModelAsync(Guid id, Guid cdpPersonId, string clientId, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationByGuidAsync(id, ct);
            if (org is null) return null;

            var user = await _adapter.GetUserAsync(org.CdpOrganisationGuid, cdpPersonId, ct);
            if (user is null) return null;

            var apps = await _adapter.GetApplicationsAsync(org.Id, ct);
            var app = apps.FirstOrDefault(a => a.Application?.ClientId == clientId);
            if (app is null || app.Application is null) return null;

            var assignments = await _adapter.GetUserAssignmentsAsync(org.Id, cdpPersonId, ct);
            var assignment = assignments.FirstOrDefault(a => a.IsActive && a.OrganisationApplicationId == app.Id);

            return new RemoveApplicationViewModel(
                OrganisationId: id,
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

        public async Task<RemoveSuccessViewModel?> GetRemoveSuccessViewModelAsync(Guid id, Guid cdpPersonId, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationByGuidAsync(id, ct);
            if (org is null) return null;

            var user = await _adapter.GetUserAsync(org.CdpOrganisationGuid, cdpPersonId, ct);
            if (user is null) return null;

            return new RemoveSuccessViewModel
            {
                OrganisationId = id,
                UserDisplayName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                OrganisationName = org.Name,
                MemberSince = user.JoinedAt?.ToString("dd MMMM yyyy") ?? string.Empty,
                Role = user.OrganisationRole,
                CdpPersonId = cdpPersonId
            };
        }

        public async Task<RemovalValidationResult> ValidateRemovalAsync(
            Guid id, Guid cdpPersonId, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationByGuidAsync(id, ct);
            if (org is null) return RemovalValidationResult.Fail("Organisation not found.");

            var user = await _adapter.GetUserAsync(org.CdpOrganisationGuid, cdpPersonId, ct);
            if (user is null) return RemovalValidationResult.Fail("User not found.");

            var currentUserEmail = _currentUserService.GetUserEmail();
            var currentUserOrgRole = _currentUserService.GetOrganisationRole(org.CdpOrganisationGuid);

            var users = await _adapter.GetUsersAsync(org.CdpOrganisationGuid, ct);
            var isLastOwner = IsLastOwner(users, cdpPersonId);

            return UserRemovalValidator.Validate(
                user.Email,
                currentUserEmail,
                user.OrganisationRole,
                isLastOwner,
                currentUserOrgRole);
        }

        public async Task<UserRemovalSubmitResult> ValidateAndRemoveUserAsync(
            Guid id,
            Guid cdpPersonId,
            bool? removeConfirmed,
            CancellationToken ct)
        {
            if (removeConfirmed == false) return new UserRemovalSubmitResult.Cancelled();

            var org = await _adapter.GetOrganisationByGuidAsync(id, ct);
            if (org is null) return new UserRemovalSubmitResult.NotFound();

            var user = await _adapter.GetUserAsync(org.CdpOrganisationGuid, cdpPersonId, ct);
            if (user is null) return new UserRemovalSubmitResult.NotFound();

            var currentUserEmail = _currentUserService.GetUserEmail();
            var currentUserOrgRole = _currentUserService.GetOrganisationRole(org.CdpOrganisationGuid);
            var users = await _adapter.GetUsersAsync(org.CdpOrganisationGuid, ct);
            var validation = UserRemovalValidator.Validate(
                user.Email,
                currentUserEmail,
                user.OrganisationRole,
                IsLastOwner(users, cdpPersonId),
                currentUserOrgRole);

            if (!validation.IsValid)
                return new UserRemovalSubmitResult.ValidationError(validation.ErrorMessage!);

            var result = await _adapter.RemoveUserAsync(org.CdpOrganisationGuid, cdpPersonId, ct);
            var success = result.Match(_ => false, outcome => outcome == ServiceOutcome.Success);
            return success ? new UserRemovalSubmitResult.Removed() : new UserRemovalSubmitResult.NotFound();
        }

        public async Task<InviteRemovalSubmitResult> RemoveInviteAsync(Guid id, Guid inviteGuid, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationByGuidAsync(id, ct);
            if (org is null) return new InviteRemovalSubmitResult.NotFound();

            var invite = await _adapter.GetInviteAsync(org.CdpOrganisationGuid, inviteGuid, ct);
            if (invite is null) return new InviteRemovalSubmitResult.NotFound();

            var result = await _adapter.CancelInviteAsync(org.CdpOrganisationGuid, invite.PendingInviteId, ct);
            var success = result.Match(_ => false, outcome => outcome == ServiceOutcome.Success);
            return success ? new InviteRemovalSubmitResult.Removed() : new InviteRemovalSubmitResult.NotFound();
        }

        public async Task<ApplicationRemovalSubmitResult> RemoveApplicationAsync(Guid id, Guid cdpPersonId, string clientId, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationByGuidAsync(id, ct);
            if (org is null) return new ApplicationRemovalSubmitResult.NotFound();

            var apps = await _adapter.GetApplicationsAsync(org.Id, ct);
            var app = apps.FirstOrDefault(a => a.Application?.ClientId == clientId);
            if (app is null) return new ApplicationRemovalSubmitResult.NotFound();

            var assignments = await _adapter.GetUserAssignmentsAsync(org.Id, cdpPersonId, ct);
            var assignment = assignments.FirstOrDefault(a => a.IsActive && a.OrganisationApplicationId == app.Id);
            if (assignment is null) return new ApplicationRemovalSubmitResult.NotFound();

            var result = await _adapter.DeleteUserAssignmentAsync(org.Id, cdpPersonId, assignment.Id, ct);
            var success = result.Match(_ => false, outcome => outcome == ServiceOutcome.Success);
            return success ? new ApplicationRemovalSubmitResult.Removed() : new ApplicationRemovalSubmitResult.NotFound();
        }

        private static bool IsLastOwner(ICollection<OrganisationUserResponse> users, Guid cdpPersonId)
        {
            var owners = users.Where(u => u.OrganisationRole == OrganisationRole.Owner).ToList();
            return owners.Count == 1 && owners[0].CdpPersonId == cdpPersonId;
        }
    }
}
