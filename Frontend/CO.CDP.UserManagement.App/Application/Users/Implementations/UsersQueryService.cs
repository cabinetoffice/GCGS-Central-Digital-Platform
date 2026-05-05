using CO.CDP.UserManagement.App.Adapters;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.Shared.Responses;

namespace CO.CDP.UserManagement.App.Application.Users.Implementations
{
    public class UsersQueryService : IUsersQueryService
    {
        private readonly IUserManagementApiAdapter _adapter;

        public UsersQueryService(IUserManagementApiAdapter adapter)
        {
            _adapter = adapter;
        }

        public async Task<UsersViewModel?> GetViewModelAsync(
            Guid id,
            string? role,
            string? application,
            string? search,
            CancellationToken ct)
        {
            if (id == Guid.Empty) return null;
            var org = await _adapter.GetOrganisationByGuidAsync(id, ct);
            if (org == null) return null;

            var usersTask = _adapter.GetUsersAsync(org.CdpOrganisationGuid, ct);
            var invitesTask = _adapter.GetInvitesAsync(org.CdpOrganisationGuid, ct);
            var applicationsTask = _adapter.GetApplicationsAsync(org.Id, ct);
            var joinRequestsTask = _adapter.GetJoinRequestsAsync(org.CdpOrganisationGuid, ct);

            await Task.WhenAll(usersTask, invitesTask, applicationsTask, joinRequestsTask);

            var users = await usersTask;
            var invites = await invitesTask;
            var applications = (await applicationsTask).ToList();
            var joinRequests = (await joinRequestsTask).ToList();

            var filter = new UsersFilter(role, application, search);

            var filteredUsers = UserFilterPipeline.ApplyTo(users, filter);
            var filteredInvites = UserFilterPipeline.ApplyTo(invites, filter, applications);

            var usersVm = filteredUsers.Select(u => new UserSummaryViewModel(
                    u.CdpPersonId ?? Guid.Empty,
                    null,
                    $"{u.FirstName} {u.LastName}",
                    u.Email,
                    u.OrganisationRole,
                    u.Status,
                    (u.ApplicationAssignments ?? Enumerable.Empty<UserAssignmentResponse>())
                    .Select(ar =>
                    {
                        var orgApp = applications.FirstOrDefault(a => a.Id == ar.OrganisationApplicationId);
                        var appName = orgApp?.Application?.Name ?? string.Empty;
                        var appSlug = orgApp?.Application?.ClientId ?? string.Empty;
                        var roleName = ar.Roles?.FirstOrDefault()?.Name ?? string.Empty;
                        return new UserApplicationAccessViewModel(appName, appSlug, roleName);
                    })
                    .ToList()))
                .ToList();

            var invitesVm = filteredInvites
                .Select(i => new UserSummaryViewModel(
                    null,
                    i.CdpPersonInviteGuid,
                    $"{i.FirstName} {i.LastName}",
                    i.Email,
                    i.OrganisationRole,
                    i.Status,
                    (i.ApplicationAssignments ?? Enumerable.Empty<InviteApplicationAssignmentResponse>())
                    .Select(ar =>
                    {
                        var orgApp = applications.FirstOrDefault(a => a.Id == ar.OrganisationApplicationId);
                        var appName = !string.IsNullOrEmpty(ar.ApplicationName)
                            ? ar.ApplicationName
                            : orgApp?.Application?.Name ?? string.Empty;
                        var appSlug = orgApp?.Application?.ClientId ?? string.Empty;
                        return new UserApplicationAccessViewModel(appName, appSlug, string.Empty);
                    })
                    .ToList()))
                .ToList();

            var appsVm = applications
                .Select(a => new ApplicationViewModel(
                    a.Application?.Id ?? 0,
                    a.Application?.ClientId ?? string.Empty,
                    a.Application?.Name ?? string.Empty,
                    a.Application?.Description ?? string.Empty,
                    a.Application?.Category ?? string.Empty,
                    a.IsActive,
                    a.Application?.IsEnabledByDefault ?? false,
                    0,
                    0))
                .ToList();

            return new UsersViewModel(
                org.Name,
                org.CdpOrganisationGuid,
                org.CdpOrganisationGuid,
                usersVm.Concat(invitesVm).ToList(),
                appsVm,
                role,
                application,
                search,
                filteredUsers.Count + filteredInvites.Count,
                joinRequests.Count > 0 ? joinRequests : null
            );
        }
    }

    public class UserDetailsQueryService : IUserDetailsQueryService
    {
        private readonly IUserManagementApiAdapter _adapter;

        public UserDetailsQueryService(IUserManagementApiAdapter adapter)
        {
            _adapter = adapter;
        }

        public async Task<UserDetailsViewModel?> GetViewModelAsync(
            Guid id,
            Guid cdpPersonId,
            CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationByGuidAsync(id, ct);
            if (org == null) return null;
            var user = await _adapter.GetUserAsync(org.CdpOrganisationGuid, cdpPersonId, ct);
            if (user == null) return null;
            // Map user to UserDetailsViewModel and include application access details
            var applications = await _adapter.GetApplicationsAsync(org.Id, ct);
            var appLookup = applications
                .ToDictionary(a => a.Id, a => a);

            var appDetails = (user.ApplicationAssignments ?? Enumerable.Empty<UserAssignmentResponse>())
                .Select(ar =>
                {
                    appLookup.TryGetValue(ar.OrganisationApplicationId, out var orgApp);
                    var appClientId = orgApp?.Application?.ClientId ?? string.Empty;
                    var permissions = (ar.Roles ?? Enumerable.Empty<RoleResponse>())
                        .SelectMany(r => r.Permissions ?? Enumerable.Empty<PermissionResponse>())
                        .Select(p => p.Name)
                        .Distinct()
                        .ToList();

                    var assignedByEmail = string.IsNullOrWhiteSpace(ar.AssignedBy) ? "System" : ar.AssignedBy;

                    return new UserApplicationAccessDetailViewModel(
                        ar.OrganisationApplicationId,
                        appClientId,
                        orgApp?.Application?.Name ?? string.Empty,
                        orgApp?.Application?.Description ?? string.Empty,
                        permissions,
                        ar.AssignedAt ?? ar.CreatedAt,
                        assignedByEmail,
                        ar.Roles?.FirstOrDefault()?.Name ?? string.Empty);
                })
                .ToList();

            return new UserDetailsViewModel(
                org,
                user.CdpPersonId ?? Guid.Empty,
                $"{user.FirstName} {user.LastName}",
                user.Email,
                user.OrganisationRole,
                user.JoinedAt?.ToString("dd MMMM yyyy") ?? "Unknown",
                appDetails
            );
        }

        public async Task<RemoveApplicationSuccessViewModel?> GetRemoveApplicationSuccessViewModelAsync(
            Guid id,
            Guid cdpPersonId,
            string clientId,
            CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationByGuidAsync(id, ct);
            if (org == null) return null;
            var user = await _adapter.GetUserAsync(org.CdpOrganisationGuid, cdpPersonId, ct);
            if (user == null) return null;

            var apps = await _adapter.GetApplicationsAsync(org.Id, ct);
            var app = apps.FirstOrDefault(a => a.Application?.ClientId == clientId);
            if (app == null || app.Application == null) return null;

            return new RemoveApplicationSuccessViewModel
            {
                OrganisationId = id,
                UserDisplayName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                ApplicationName = app.Application.Name,
                CdpPersonId = cdpPersonId
            };
        }
    }

    public class InviteDetailsQueryService : IInviteDetailsQueryService
    {
        private readonly IUserManagementApiAdapter _adapter;
        public InviteDetailsQueryService(IUserManagementApiAdapter adapter) => _adapter = adapter;

        public async Task<InviteDetailsViewModel?> GetViewModelAsync(
            Guid id, Guid inviteGuid, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationByGuidAsync(id, ct);
            if (org == null) return null;
            var invite = await _adapter.GetInviteAsync(org.CdpOrganisationGuid, inviteGuid, ct);
            if (invite == null) return null;

            var appNames = (invite.ApplicationAssignments ?? Enumerable.Empty<InviteApplicationAssignmentResponse>())
                .Select(ar => ar.ApplicationName)
                .Where(n => !string.IsNullOrEmpty(n))
                .ToList();

            return new InviteDetailsViewModel(
                org,
                invite.CdpPersonInviteGuid,
                invite.PendingInviteId,
                $"{invite.FirstName} {invite.LastName}".Trim(),
                invite.Email,
                invite.OrganisationRole,
                invite.CreatedAt,
                appNames);
        }
    }
}