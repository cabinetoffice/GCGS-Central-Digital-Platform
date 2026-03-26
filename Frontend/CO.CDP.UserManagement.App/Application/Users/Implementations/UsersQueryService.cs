using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Adapters;
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
            string? organisationSlug,
            string? role,
            string? application,
            string? search,
            CancellationToken ct)
        {
            if (string.IsNullOrEmpty(organisationSlug)) return null;
            var org = await _adapter.GetOrganisationBySlugAsync(organisationSlug, ct);
            if (org == null) return null;
            // Fetch users, invites and applications in parallel
            var usersTask = _adapter.GetUsersAsync(org.CdpOrganisationGuid, ct);
            var invitesTask = _adapter.GetInvitesAsync(org.CdpOrganisationGuid, ct);
            var applicationsTask = _adapter.GetApplicationsAsync(org.Id, ct);
            await Task.WhenAll(usersTask, invitesTask, applicationsTask);

            var users = usersTask.Result ?? new List<OrganisationUserResponse>();
            var invites = invitesTask.Result ?? new List<PendingOrganisationInviteResponse>();
            var applications = applicationsTask.Result ?? new List<OrganisationApplicationResponse>();

            // Apply filters
            IEnumerable<OrganisationUserResponse> filteredUsers = users;
            if (!string.IsNullOrEmpty(role))
            {
                filteredUsers = filteredUsers.Where(u => string.Equals(u.OrganisationRole.ToString(), role, StringComparison.OrdinalIgnoreCase));
                invites = (invites ?? Enumerable.Empty<PendingOrganisationInviteResponse>())
                    .Where(i => string.Equals(i.OrganisationRole.ToString(), role, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            if (!string.IsNullOrEmpty(search))
            {
                var lower = search.ToLowerInvariant();
                filteredUsers = filteredUsers.Where(u =>
                    ($"{u.FirstName} {u.LastName}".ToLowerInvariant().Contains(lower)) ||
                    (u.Email?.ToLowerInvariant().Contains(lower) == true));
            }
            // Application filter: best-effort match against user's ApplicationRoles (by ApplicationId or ClientId)
            if (!string.IsNullOrEmpty(application))
            {
                filteredUsers = filteredUsers.Where(u =>
                    (u.ApplicationAssignments ?? Enumerable.Empty<UserAssignmentResponse>())
                        .Any(ar => string.Equals((ar.ApplicationId ?? ar.OrganisationApplicationId).ToString(), application, StringComparison.OrdinalIgnoreCase)
                                   || string.Equals(ar.Application?.ClientId, application, StringComparison.OrdinalIgnoreCase)));
            }

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

            var invitesVm = (invites ?? Enumerable.Empty<PendingOrganisationInviteResponse>())
                .Where(i => string.IsNullOrEmpty(search) || (i.Email.Contains(search, StringComparison.OrdinalIgnoreCase)) || ($"{i.FirstName} {i.LastName}".Contains(search, StringComparison.OrdinalIgnoreCase)))
                .Select(i => new UserSummaryViewModel(
                    null,
                    i.CdpPersonInviteGuid,
                    $"{i.FirstName} {i.LastName}",
                    i.Email,
                    i.OrganisationRole,
                    i.Status,
                    new List<UserApplicationAccessViewModel>()))
                .ToList();

            var appsVm = (applications ?? Enumerable.Empty<OrganisationApplicationResponse>())
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
                org.Slug,
                org.CdpOrganisationGuid,
                usersVm.Concat(invitesVm).ToList(),
                appsVm,
                role,
                application,
                search,
                users.Count
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
            string organisationSlug,
            Guid cdpPersonId,
            CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationBySlugAsync(organisationSlug, ct);
            if (org == null) return null;
            var user = await _adapter.GetUserAsync(org.CdpOrganisationGuid, cdpPersonId, ct);
            if (user == null) return null;
            // Map user to UserDetailsViewModel and include application access details
            var applications = await _adapter.GetApplicationsAsync(org.Id, ct);
            var appLookup = (applications ?? Enumerable.Empty<OrganisationApplicationResponse>())
                .ToDictionary(a => a.Id, a => a);

            var appDetails = (user.ApplicationAssignments ?? Enumerable.Empty<UserAssignmentResponse>())
                .Select(ar =>
                {
                    appLookup.TryGetValue(ar.OrganisationApplicationId, out var orgApp);
                    return new UserApplicationAccessDetailViewModel(
                        ar.OrganisationApplicationId,
                        orgApp?.Application?.Name ?? string.Empty,
                        orgApp?.Application?.Description ?? string.Empty,
                        (IReadOnlyList<string>)(new List<string>()),
                        ar.AssignedAt ?? ar.CreatedAt,
                        ar.AssignedBy ?? string.Empty,
                        ar.Roles?.FirstOrDefault()?.Name ?? string.Empty);

                }).ToList();

            return new UserDetailsViewModel(
                org,
                user.CdpPersonId ?? Guid.Empty,
                user.FirstName + " " + user.LastName,
                user.Email,
                user.OrganisationRole,
                user.JoinedAt?.ToString("MMMM dd, yyyy") ?? "Unknown",
                appDetails
            );
        }

        public async Task<RemoveApplicationSuccessViewModel?> GetRemoveApplicationSuccessViewModelAsync(
            string organisationSlug,
            Guid cdpPersonId,
            string clientId,
            CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationBySlugAsync(organisationSlug, ct);
            if (org == null) return null;
            var user = await _adapter.GetUserAsync(org.CdpOrganisationGuid, cdpPersonId, ct);
            if (user == null) return null;

            var apps = await _adapter.GetApplicationsAsync(org.Id, ct);
            var app = apps.FirstOrDefault(a => a.Application?.ClientId == clientId);
            if (app == null || app.Application == null) return null;

            return new RemoveApplicationSuccessViewModel
            {
                OrganisationSlug = organisationSlug,
                UserDisplayName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                ApplicationName = app.Application.Name ?? string.Empty,
                CdpPersonId = cdpPersonId
            };
        }
    }
}
