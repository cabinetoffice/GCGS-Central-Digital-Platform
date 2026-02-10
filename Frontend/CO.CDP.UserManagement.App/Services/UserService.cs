using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.Shared.Enums;
using ApiClient = CO.CDP.UserManagement.WebApiClient;

namespace CO.CDP.UserManagement.App.Services;

public sealed class UserService(ApiClient.UserManagementClient apiClient) : IUserService
{
    public async Task<UsersViewModel?> GetUsersViewModelAsync(
        string organisationSlug,
        string? selectedRole = null,
        string? selectedApplication = null,
        string? searchTerm = null,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            var usersResponse = await apiClient.UsersAll2Async(org.CdpOrganisationGuid, ct);
            var invitesResponse = await apiClient.InvitesAllAsync(org.CdpOrganisationGuid, ct);

            var users = usersResponse
                .Select(user => new UserSummaryViewModel(
                    Id: user.CdpPersonId,
                    PendingInviteId: null,
                    Name: !string.IsNullOrWhiteSpace(user.FirstName) && !string.IsNullOrWhiteSpace(user.LastName)
                        ? $"{user.FirstName} {user.LastName}"
                        : string.Empty,
                    Email: user.Email ?? string.Empty,
                    OrganisationRole: user.OrganisationRole,
                    Status: user.Status,
                    ApplicationAccess: user.ApplicationAssignments?
                        .Where(assignment => assignment.Application != null)
                        .Select(assignment => new UserApplicationAccessViewModel(
                            ApplicationName: assignment.Application!.Name,
                            ApplicationSlug: assignment.Application!.ClientId,
                            RoleName: assignment.Roles == null
                                ? string.Empty
                                : string.Join(", ", assignment.Roles.Select(role => role.Name))))
                        .ToList() ?? []))
                .ToList();

            var pendingInvites = invitesResponse
                .Select(invite =>
                {
                    var displayName = !string.IsNullOrWhiteSpace(invite.FirstName) && !string.IsNullOrWhiteSpace(invite.LastName)
                        ? $"{invite.FirstName} {invite.LastName}"
                        : string.Empty;
                    return new UserSummaryViewModel(
                        Id: null,
                        PendingInviteId: invite.PendingInviteId,
                        Name: displayName,
                        Email: invite.Email,
                        OrganisationRole: null,
                        Status: invite.Status,
                        ApplicationAccess: []);
                })
                .ToList();

            var allUsers = users.Concat(pendingInvites).ToList();
            var filteredUsers = allUsers
                .Where(user => string.IsNullOrWhiteSpace(selectedRole) ||
                               (user.OrganisationRole?.ToString() ?? string.Empty)
                               .Equals(selectedRole, StringComparison.OrdinalIgnoreCase))
                .Where(user => string.IsNullOrWhiteSpace(selectedApplication) ||
                               user.ApplicationAccess.Any(app =>
                                   app.ApplicationSlug.Equals(selectedApplication, StringComparison.OrdinalIgnoreCase)))
                .Where(user => string.IsNullOrWhiteSpace(searchTerm) ||
                               user.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                               user.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return new UsersViewModel(
                OrganisationName: org.Name,
                OrganisationSlug: org.Slug,
                Users: filteredUsers,
                SelectedRole: selectedRole,
                SelectedApplication: selectedApplication,
                SearchTerm: searchTerm,
                TotalCount: filteredUsers.Count()
            );
        }
        catch (ApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<InviteUserViewModel?> GetInviteUserViewModelAsync(
        string organisationSlug,
        InviteUserViewModel? input = null,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            return new InviteUserViewModel
            {
                OrganisationName = org.Name,
                OrganisationSlug = org.Slug,
                FirstName = input?.FirstName,
                LastName = input?.LastName,
                Email = input?.Email,
                OrganisationRole = input?.OrganisationRole
            };
        }
        catch (ApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<bool> InviteUserAsync(
        string organisationSlug,
        InviteUserViewModel input,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            var request = new ApiClient.InviteUserRequest(
                input.Email ?? string.Empty,
                input.FirstName ?? string.Empty,
                input.LastName ?? string.Empty,
                input.OrganisationRole ?? OrganisationRole.Member);

            await apiClient.InvitesAsync(org.CdpOrganisationGuid, request, ct);
            return true;
        }
        catch (ApiClient.ApiException)
        {
            return false;
        }
    }

    public async Task<ChangeUserRoleViewModel?> GetChangeUserRoleViewModelAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        int? pendingInviteId,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);

            if (pendingInviteId.HasValue)
            {
                var invites = await apiClient.InvitesAllAsync(org.CdpOrganisationGuid, ct);
                var invite = invites.FirstOrDefault(i => i.PendingInviteId == pendingInviteId.Value);
                if (invite == null) return null;

                var displayName = !string.IsNullOrWhiteSpace(invite.FirstName) && !string.IsNullOrWhiteSpace(invite.LastName)
                    ? $"{invite.FirstName} {invite.LastName}"
                    : string.Empty;
                return new ChangeUserRoleViewModel(
                    OrganisationName: org.Name,
                    OrganisationSlug: org.Slug,
                    UserDisplayName: displayName,
                    Email: invite.Email,
                    CurrentRole: invite.OrganisationRole,
                    SelectedRole: invite.OrganisationRole,
                    IsPending: true,
                    CdpPersonId: null,
                    PendingInviteId: invite.PendingInviteId);
            }

            if (cdpPersonId.HasValue)
            {
                var users = await apiClient.UsersAll2Async(org.CdpOrganisationGuid, ct);
                var user = users.FirstOrDefault(u => u.CdpPersonId == cdpPersonId);
                if (user == null) return null;

                var displayName = !string.IsNullOrWhiteSpace(user.FirstName) && !string.IsNullOrWhiteSpace(user.LastName)
                    ? $"{user.FirstName} {user.LastName}"
                    : string.Empty;
                return new ChangeUserRoleViewModel(
                    OrganisationName: org.Name,
                    OrganisationSlug: org.Slug,
                    UserDisplayName: displayName,
                    Email: user.Email ?? string.Empty,
                    CurrentRole: user.OrganisationRole,
                    SelectedRole: user.OrganisationRole,
                    IsPending: false,
                    CdpPersonId: user.CdpPersonId,
                    PendingInviteId: null);
            }

            return null;
        }
        catch (ApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<bool> UpdateUserRoleAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        int? pendingInviteId,
        OrganisationRole organisationRole,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            var request = new ApiClient.ChangeOrganisationRoleRequest(organisationRole);

            if (pendingInviteId.HasValue)
            {
                await apiClient.RoleAsync(org.CdpOrganisationGuid, pendingInviteId.Value, request, ct);
                return true;
            }

            if (cdpPersonId.HasValue)
            {
                await apiClient.Role2Async(org.CdpOrganisationGuid, cdpPersonId.Value, request, ct);
                return true;
            }

            return false;
        }
        catch (ApiClient.ApiException)
        {
            return false;
        }
    }

    public async Task<bool> ResendInviteAsync(
        string organisationSlug,
        int pendingInviteId,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            await apiClient.ResendAsync(org.CdpOrganisationGuid, pendingInviteId, ct);
            return true;
        }
        catch (ApiClient.ApiException)
        {
            return false;
        }
    }
}
