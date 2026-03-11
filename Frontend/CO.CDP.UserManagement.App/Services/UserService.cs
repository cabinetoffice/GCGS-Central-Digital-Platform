using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
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
                    InviteGuid: null,
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
                        InviteGuid: invite.CdpPersonInviteGuid,
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
        CancellationToken ct = default,
        IReadOnlyList<InviteApplicationAssignment>? applicationAssignments = null)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            var assignments = applicationAssignments?
                .Select(a => new ApiClient.ApplicationAssignment(
                    new List<int> { a.ApplicationRoleId },
                    a.OrganisationApplicationId))
                .ToList() ?? new List<ApiClient.ApplicationAssignment>();
            var request = new ApiClient.InviteUserRequest(
                assignments,
                input.Email ?? string.Empty,
                input.FirstName ?? string.Empty,
                input.LastName ?? string.Empty,
                input.OrganisationRole ?? OrganisationRole.Member);

            await apiClient.InvitesPOSTAsync(org.CdpOrganisationGuid, request, ct);
            return true;
        }
        catch (ApiClient.ApiException)
        {
            return false;
        }
    }

    public async Task<ApplicationRolesStepViewModel?> GetApplicationRolesStepViewModelAsync(
        string organisationSlug,
        InviteUserState state,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            var enabledApps = (await apiClient.ApplicationsAllAsync(org.Id, ct))
                .Where(app => app.IsActive && app.Application != null)
                .ToList();
            var applicationSelections = new List<ApplicationAccessSelectionViewModel>();

            foreach (var enabledApp in enabledApps)
            {
                ICollection<RoleResponse> roles;
                try
                {
                    roles = await apiClient.RolesAllAsync(enabledApp.ApplicationId, ct);
                }
                catch (ApiClient.ApiException ex) when (ex.StatusCode == 404)
                {
                    roles = [];
                }

                applicationSelections.Add(new ApplicationAccessSelectionViewModel
                {
                    OrganisationApplicationId = enabledApp.Id,
                    ApplicationName = enabledApp.Application?.Name ?? string.Empty,
                    ApplicationDescription = enabledApp.Application?.Description ?? string.Empty,
                    Roles = roles.Select(role => new ApplicationRoleOptionViewModel
                    {
                        Id = role.Id,
                        Name = role.Name,
                        Description = role.Description
                    }).ToList()
                });
            }

            return new ApplicationRolesStepViewModel
            {
                OrganisationSlug = organisationSlug,
                FirstName = state.FirstName,
                LastName = state.LastName,
                Email = state.Email,
                OrganisationRole = state.OrganisationRole,
                Applications = applicationSelections
            };
        }
        catch (ApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<ChangeUserRoleViewModel?> GetChangeUserRoleViewModelAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);

            if (inviteGuid.HasValue)
            {
                var invites = await apiClient.InvitesAllAsync(org.CdpOrganisationGuid, ct);
                var invite = invites.FirstOrDefault(i => i.CdpPersonInviteGuid == inviteGuid.Value);
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
                    InviteGuid: invite.CdpPersonInviteGuid);
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
                    InviteGuid: null);
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
        Guid? inviteGuid,
        OrganisationRole organisationRole,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            var request = new ApiClient.ChangeOrganisationRoleRequest(organisationRole);

            if (inviteGuid.HasValue)
            {
                var invites = await apiClient.InvitesAllAsync(org.CdpOrganisationGuid, ct);
                var invite = invites.FirstOrDefault(i => i.CdpPersonInviteGuid == inviteGuid.Value);
                if (invite is null)
                {
                    return false;
                }

                await apiClient.RoleAsync(org.CdpOrganisationGuid, invite.PendingInviteId, request, ct);
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
        Guid inviteGuid,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            var invites = await apiClient.InvitesAllAsync(org.CdpOrganisationGuid, ct);
            var invite = invites.FirstOrDefault(item => item.CdpPersonInviteGuid == inviteGuid);
            if (invite == null)
            {
                return false;
            }

            var request = new ApiClient.InviteUserRequest(
                new List<ApiClient.ApplicationAssignment>(),
                invite.Email,
                invite.FirstName,
                invite.LastName,
                invite.OrganisationRole);

            await apiClient.InvitesPOSTAsync(org.CdpOrganisationGuid, request, ct);
            await apiClient.InvitesDELETEAsync(org.CdpOrganisationGuid, invite.PendingInviteId, ct);
            return true;
        }
        catch (ApiClient.ApiException)
        {
            return false;
        }
    }

    public async Task<ChangeUserApplicationRolesViewModel?> GetChangeUserApplicationRolesViewModelAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            string userDisplayName = string.Empty;
            string userEmail = string.Empty;
            bool isPending = false;
            var applicationRoleChanges = new List<ApplicationRoleChangeViewModel>();

            if (inviteGuid.HasValue)
            {
                var invites = await apiClient.InvitesAllAsync(org.CdpOrganisationGuid, ct);
                var invite = invites.FirstOrDefault(i => i.CdpPersonInviteGuid == inviteGuid.Value);
                if (invite == null) return null;

                userDisplayName = !string.IsNullOrWhiteSpace(invite.FirstName) && !string.IsNullOrWhiteSpace(invite.LastName)
                    ? $"{invite.FirstName} {invite.LastName}"
                    : string.Empty;
                userEmail = invite.Email;
                isPending = true;

                foreach (var assignment in invite.ApplicationAssignments ?? [])
                {
                    ICollection<RoleResponse> roles;
                    try
                    {
                        roles = await apiClient.RolesAllAsync(assignment.ApplicationId ?? 0, ct);
                    }
                    catch (ApiClient.ApiException ex) when (ex.StatusCode == 404)
                    {
                        roles = [];
                    }

                    applicationRoleChanges.Add(new ApplicationRoleChangeViewModel
                    {
                        OrganisationApplicationId = assignment.OrganisationApplicationId,
                        ApplicationName = assignment.ApplicationName,
                        ApplicationDescription = string.Empty,
                        SelectedRoleId = assignment.ApplicationRoleId,
                        Roles = roles.Select(role => new ApplicationRoleOptionViewModel
                        {
                            Id = role.Id,
                            Name = role.Name,
                            Description = role.Description
                        }).ToList()
                    });
                }
            }
            else if (cdpPersonId.HasValue)
            {
                var user = await apiClient.Users2Async(org.CdpOrganisationGuid, cdpPersonId.Value, ct);
                if (user == null) return null;

                userDisplayName = !string.IsNullOrWhiteSpace(user.FirstName) && !string.IsNullOrWhiteSpace(user.LastName)
                    ? $"{user.FirstName} {user.LastName}"
                    : string.Empty;
                userEmail = user.Email ?? string.Empty;
                isPending = false;

                foreach (var assignment in user.ApplicationAssignments ?? [])
                {
                    if (assignment.Application == null) continue;

                    ICollection<RoleResponse> roles;
                    try
                    {
                        roles = await apiClient.RolesAllAsync(assignment.ApplicationId ?? 0, ct);
                    }
                    catch (ApiClient.ApiException ex) when (ex.StatusCode == 404)
                    {
                        roles = [];
                    }

                    var currentRoleId = assignment.Roles?.FirstOrDefault()?.Id;

                    applicationRoleChanges.Add(new ApplicationRoleChangeViewModel
                    {
                        OrganisationApplicationId = assignment.OrganisationApplicationId,
                        ApplicationName = assignment.Application.Name ?? string.Empty,
                        ApplicationDescription = string.Empty,
                        SelectedRoleId = currentRoleId,
                        Roles = roles.Select(role => new ApplicationRoleOptionViewModel
                        {
                            Id = role.Id,
                            Name = role.Name,
                            Description = role.Description
                        }).ToList()
                    });
                }
            }
            else
            {
                return null;
            }

            return new ChangeUserApplicationRolesViewModel
            {
                OrganisationSlug = organisationSlug,
                UserDisplayName = userDisplayName,
                Email = userEmail,
                IsPending = isPending,
                CdpPersonId = cdpPersonId,
                InviteGuid = inviteGuid,
                Applications = applicationRoleChanges
            };
        }
        catch (ApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<bool> UpdateUserApplicationRolesAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        IReadOnlyList<ApplicationRoleAssignmentPostModel> applicationRoleAssignments,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);

            if (cdpPersonId.HasValue)
            {
                var users = await apiClient.UsersAll2Async(org.CdpOrganisationGuid, ct);
                var user = users.FirstOrDefault(u => u.CdpPersonId == cdpPersonId);
                if (user == null) return false;

                // Update each application assignment with the selected role
                foreach (var assignment in applicationRoleAssignments)
                {
                    if (assignment.SelectedRoleId.HasValue)
                    {
                        var request = new UpdateAssignmentRolesRequest
                        {
                            RoleIds = [assignment.SelectedRoleId.Value]
                        };

                        // Get the user's user principal ID for the API call
                        var userPrincipalId = user.Email ?? string.Empty;
                        await apiClient.AssignmentsPUTAsync(
                            org.Id,
                            userPrincipalId,
                            user.ApplicationAssignments
                                ?.FirstOrDefault(a => a.OrganisationApplicationId == assignment.OrganisationApplicationId)
                                ?.Id ?? 0,
                            request,
                            ct);
                    }
                }

                return true;
            }

            if (inviteGuid.HasValue)
            {
                var invites = await apiClient.InvitesAllAsync(org.CdpOrganisationGuid, ct);
                var invite = invites.FirstOrDefault(i => i.CdpPersonInviteGuid == inviteGuid);
                if (invite == null) return false;

                var assignments = applicationRoleAssignments
                    .Where(a => a.SelectedRoleId.HasValue)
                    .Select(a => new ApiClient.ApplicationAssignment(
                        new List<int> { a.SelectedRoleId!.Value },
                        a.OrganisationApplicationId))
                    .ToList();

                var newInviteRequest = new ApiClient.InviteUserRequest(
                    assignments,
                    invite.Email,
                    invite.FirstName ?? string.Empty,
                    invite.LastName ?? string.Empty,
                    invite.OrganisationRole);

                await apiClient.InvitesPOSTAsync(org.CdpOrganisationGuid, newInviteRequest, ct);
                await apiClient.InvitesDELETEAsync(org.CdpOrganisationGuid, invite.PendingInviteId, ct);
                return true;
            }

            return false;
        }
        catch (ApiClient.ApiException)
        {
            return false;
        }
    }
}
