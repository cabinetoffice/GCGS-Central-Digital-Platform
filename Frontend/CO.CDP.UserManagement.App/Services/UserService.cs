using CO.CDP.UserManagement.App.Models;
using CO.CDP.Functional;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using ApiClient = CO.CDP.UserManagement.WebApiClient;
using ClientPartyRole = CO.CDP.UserManagement.WebApiClient.PartyRole;

namespace CO.CDP.UserManagement.App.Services;

public sealed class UserService(
    ApiClient.UserManagementClient apiClient) : IUserService
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

            var inviteAppIds = invitesResponse
                .SelectMany(i => i.ApplicationAssignments ?? [])
                .Where(a => a.ApplicationId.HasValue)
                .Select(a => a.ApplicationId!.Value)
                .Distinct()
                .ToList();

            var rolesByAppId = new Dictionary<int, ICollection<RoleResponse>>();
            foreach (var appId in inviteAppIds)
            {
                try
                {
                    rolesByAppId[appId] = await apiClient.RolesAllAsync(appId, ct);
                }
                catch (ApiClient.ApiException ex) when (ex.StatusCode == 404)
                {
                    rolesByAppId[appId] = [];
                }
            }

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
                    ApplicationAccess: BuildApplicationAccess(user.ApplicationAssignments)))
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
                        ApplicationAccess: (invite.ApplicationAssignments ?? [])
                            .Select(assignment =>
                            {
                                var roleName = assignment.ApplicationId.HasValue &&
                                               rolesByAppId.TryGetValue(assignment.ApplicationId.Value, out var roles)
                                    ? roles.FirstOrDefault(r => r.Id == assignment.ApplicationRoleId)?.Name ?? string.Empty
                                    : string.Empty;
                                return new UserApplicationAccessViewModel(
                                    ApplicationName: assignment.ApplicationName,
                                    ApplicationSlug: string.Empty,
                                    RoleName: roleName);
                            })
                            .ToList());
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
                OrganisationGuid: org.CdpOrganisationGuid,
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

    public async Task<Result<ServiceFailure, ServiceOutcome>> InviteUserAsync(
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
                    a.ApplicationRoleIds is { Count: > 0 }
                        ? a.ApplicationRoleIds.ToList()
                        : new List<int> { a.ApplicationRoleId },
                    a.OrganisationApplicationId))
                .ToList() ?? new List<ApiClient.ApplicationAssignment>();
            var request = new ApiClient.InviteUserRequest(
                assignments,
                input.Email ?? string.Empty,
                input.FirstName ?? string.Empty,
                input.LastName ?? string.Empty,
                input.OrganisationRole ?? OrganisationRole.Member);

            await apiClient.InvitesPOSTAsync(org.CdpOrganisationGuid, request, ct);
            return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success);
        }
        catch (ApiClient.ApiException ex)
        {
            return ServiceResultMapper.FromApiException(ex);
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
                    roles = await apiClient.RolesAll2Async(org.Id, enabledApp.ApplicationId, state.OrganisationRole, ct);
                }
                catch (ApiClient.ApiException ex) when (ex.StatusCode == 404)
                {
                    roles = [];
                }

                var allowsMultiple = enabledApp.Application?.AllowsMultipleRoleAssignments ?? false;

                applicationSelections.Add(new ApplicationAccessSelectionViewModel
                {
                    OrganisationApplicationId = enabledApp.Id,
                    ApplicationName = enabledApp.Application?.Name ?? string.Empty,
                    ApplicationDescription = enabledApp.Application?.Description ?? string.Empty,
                    AllowsMultipleRoleAssignments = allowsMultiple,
                    IsEnabledByDefault = enabledApp.Application?.IsEnabledByDefault ?? false,
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

    public async Task<Result<ServiceFailure, ServiceOutcome>> UpdateUserRoleAsync(
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
                    return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);
                }

                await apiClient.RoleAsync(org.CdpOrganisationGuid, invite.PendingInviteId, request, ct);
                return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success);
            }

            if (cdpPersonId.HasValue)
            {
                await apiClient.Role2Async(org.CdpOrganisationGuid, cdpPersonId.Value, request, ct);
                return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success);
            }

            return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);
        }
        catch (ApiClient.ApiException ex)
        {
            return ServiceResultMapper.FromApiException(ex);
        }
    }

    public async Task<Result<ServiceFailure, ServiceOutcome>> ResendInviteAsync(
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
                return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);
            }

            var request = new ApiClient.InviteUserRequest(
                new List<ApiClient.ApplicationAssignment>(),
                invite.Email,
                invite.FirstName,
                invite.LastName,
                invite.OrganisationRole);

            await apiClient.InvitesPOSTAsync(org.CdpOrganisationGuid, request, ct);
            await apiClient.InvitesDELETEAsync(org.CdpOrganisationGuid, invite.PendingInviteId, ct);
            return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success);
        }
        catch (ApiClient.ApiException ex)
        {
            return ServiceResultMapper.FromApiException(ex);
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

            var enabledApps = (await apiClient.ApplicationsAllAsync(org.Id, ct))
                .Where(app => app.IsActive && app.Application != null)
                .ToList();

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

                var assignedByOrgAppId = (invite.ApplicationAssignments ?? [])
                    .ToLookup(a => a.OrganisationApplicationId);

                foreach (var enabledApp in enabledApps)
                {
                    ICollection<RoleResponse> roles;
                    try
                    {
                        roles = await apiClient.RolesAll2Async(org.Id, enabledApp.ApplicationId, invite.OrganisationRole, ct);
                    }
                    catch (ApiClient.ApiException ex) when (ex.StatusCode == 404)
                    {
                        roles = [];
                    }

                    var allowsMultiple = enabledApp.Application?.AllowsMultipleRoleAssignments ?? false;
                    var assignedGroup = assignedByOrgAppId[enabledApp.Id].ToList();
                    var hasAccess = assignedGroup.Count > 0;
                    var assignedRoleIds = assignedGroup
                        .Select(a => a.ApplicationRoleId)
                        .Where(id => id != 0)
                        .Distinct()
                        .ToList();

                    applicationRoleChanges.Add(new ApplicationRoleChangeViewModel
                    {
                        OrganisationApplicationId = enabledApp.Id,
                        ApplicationId = enabledApp.ApplicationId,
                        ApplicationClientId = enabledApp.Application?.ClientId ?? string.Empty,
                        ApplicationName = enabledApp.Application?.Name ?? string.Empty,
                        ApplicationDescription = enabledApp.Application?.Description ?? string.Empty,
                        AllowsMultipleRoleAssignments = allowsMultiple,
                        IsEnabledByDefault = enabledApp.Application?.IsEnabledByDefault ?? false,
                        HasExistingAccess = hasAccess,
                        GiveAccess = hasAccess,
                        SelectedRoleId = assignedRoleIds.Count > 0 ? assignedRoleIds[0] : null,
                        SelectedRoleIds = assignedRoleIds,
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

                var assignedByOrgAppId = (user.ApplicationAssignments ?? [])
                    .Where(a => a.Application != null)
                    .ToDictionary(a => a.OrganisationApplicationId);

                foreach (var enabledApp in enabledApps)
                {
                    ICollection<RoleResponse> roles;
                    try
                    {
                        roles = await apiClient.RolesAll2Async(org.Id, enabledApp.ApplicationId, user.OrganisationRole, ct);
                    }
                    catch (ApiClient.ApiException ex) when (ex.StatusCode == 404)
                    {
                        roles = [];
                    }

                    var allowsMultiple = enabledApp.Application?.AllowsMultipleRoleAssignments ?? false;
                    var hasAccess = assignedByOrgAppId.TryGetValue(enabledApp.Id, out var assignment);
                    var currentRoleIds = hasAccess
                        ? (assignment!.Roles ?? []).Select(r => r.Id).ToList()
                        : new List<int>();
                    var currentRoleId = currentRoleIds.Count > 0 ? (int?)currentRoleIds[0] : null;

                    applicationRoleChanges.Add(new ApplicationRoleChangeViewModel
                    {
                        OrganisationApplicationId = enabledApp.Id,
                        ApplicationId = enabledApp.ApplicationId,
                        ApplicationClientId = enabledApp.Application?.ClientId ?? string.Empty,
                        ApplicationName = enabledApp.Application?.Name ?? string.Empty,
                        ApplicationDescription = enabledApp.Application?.Description ?? string.Empty,
                        AllowsMultipleRoleAssignments = allowsMultiple,
                        IsEnabledByDefault = enabledApp.Application?.IsEnabledByDefault ?? false,
                        HasExistingAccess = hasAccess,
                        GiveAccess = hasAccess,
                        SelectedRoleId = currentRoleId,
                        SelectedRoleIds = currentRoleIds,
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

    public async Task<Result<ServiceFailure, ServiceOutcome>> UpdateUserApplicationRolesAsync(
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
                if (user == null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

                var userPrincipalId = cdpPersonId.Value.ToString();

                foreach (var assignment in applicationRoleAssignments)
                {
                    var roleIds = GetEffectiveRoleIds(assignment);
                    if (roleIds.Count == 0) continue;

                    var existingAssignment = user.ApplicationAssignments
                        ?.FirstOrDefault(a => a.OrganisationApplicationId == assignment.OrganisationApplicationId);

                    if (existingAssignment != null)
                    {
                        var updateRequest = new UpdateAssignmentRolesRequest { RoleIds = roleIds };
                        await apiClient.AssignmentsPUTAsync(
                            org.Id,
                            userPrincipalId,
                            existingAssignment.Id,
                            updateRequest,
                            ct);
                    }
                    else
                    {
                        var createRequest = new AssignUserToApplicationRequest
                        {
                            ApplicationId = assignment.ApplicationId,
                            RoleIds = roleIds
                        };
                        await apiClient.AssignmentsPOSTAsync(org.Id, userPrincipalId, createRequest, ct);
                    }
                }

                return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success);
            }

            if (inviteGuid.HasValue)
            {
                var invites = await apiClient.InvitesAllAsync(org.CdpOrganisationGuid, ct);
                var invite = invites.FirstOrDefault(i => i.CdpPersonInviteGuid == inviteGuid);
                if (invite == null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

                var assignments = applicationRoleAssignments
                    .Select(a => (roleIds: GetEffectiveRoleIds(a), orgAppId: a.OrganisationApplicationId))
                    .Where(x => x.roleIds.Count > 0)
                    .Select(x => new ApiClient.ApplicationAssignment(x.roleIds, x.orgAppId))
                    .ToList();

                var newInviteRequest = new ApiClient.InviteUserRequest(
                    assignments,
                    invite.Email,
                    invite.FirstName ?? string.Empty,
                    invite.LastName ?? string.Empty,
                    invite.OrganisationRole);

                await apiClient.InvitesPOSTAsync(org.CdpOrganisationGuid, newInviteRequest, ct);
                await apiClient.InvitesDELETEAsync(org.CdpOrganisationGuid, invite.PendingInviteId, ct);
                return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success);
            }

            return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);
        }
        catch (ApiClient.ApiException ex)
        {
            return ServiceResultMapper.FromApiException(ex);
        }
    }

    public async Task<RemoveApplicationSuccessViewModel?> GetRemoveApplicationSuccessViewModelAsync(
        string organisationSlug,
        Guid cdpPersonId,
        string clientId,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            var users = await apiClient.UsersAll2Async(org.CdpOrganisationGuid, ct);
            var user = users.FirstOrDefault(u => u.CdpPersonId == cdpPersonId);

            if (user == null) return null;

            var allApps = await apiClient.ApplicationsAllAsync(ct);
            var app = allApps.FirstOrDefault(a => a.ClientId == clientId);

            if (app == null) return null;

            var displayName = !string.IsNullOrWhiteSpace(user.FirstName) && !string.IsNullOrWhiteSpace(user.LastName)
                ? $"{user.FirstName} {user.LastName}"
                : string.Empty;

            return new RemoveApplicationSuccessViewModel
            {
                OrganisationSlug = organisationSlug,
                UserDisplayName = displayName,
                Email = user.Email ?? string.Empty,
                ApplicationName = app.Name,
                CdpPersonId = cdpPersonId
            };
        }
        catch (ApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }


    private static IReadOnlyList<UserApplicationAccessViewModel> BuildApplicationAccess(
        IEnumerable<UserAssignmentResponse>? assignments)
    {
        return (assignments ?? [])
            .Where(assignment => assignment.Application != null)
            .GroupBy(assignment => assignment.OrganisationApplicationId)
            .Select(group =>
            {
                var first = group.First();
                var roleNames = group
                    .SelectMany(assignment => assignment.Roles ?? [])
                    .Select(role => role.Name)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct(StringComparer.OrdinalIgnoreCase);

                return new UserApplicationAccessViewModel(
                    ApplicationName: first.Application!.Name,
                    ApplicationSlug: first.Application.ClientId,
                    RoleName: string.Join(", ", roleNames));
            })
            .ToList();
    }

    private async Task<ICollection<ClientPartyRole>> GetOrgPartyRolesAsync(Guid cdpOrganisationGuid, CancellationToken ct)
    {
        try
        {
            return await apiClient.PartyRolesAsync(cdpOrganisationGuid, ct);
        }
        catch (ApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return [];
        }
    }

    private static IEnumerable<RoleResponse> FilterRolesByPartyRole(
        IEnumerable<RoleResponse> roles,
        ICollection<ClientPartyRole> orgPartyRoles)
    {
        return roles.Where(r =>
            r.RequiredPartyRoles == null ||
            !r.RequiredPartyRoles.Any() ||
            r.RequiredPartyRoles.Any(rpr => orgPartyRoles.Any(opr => (int)opr == (int)rpr)));
    }

    private static List<int> GetEffectiveRoleIds(ApplicationRoleAssignmentPostModel assignment)
    {
        if (assignment.SelectedRoleIds.Count > 0)
            return assignment.SelectedRoleIds;
        if (assignment.SelectedRoleId.HasValue)
            return [assignment.SelectedRoleId.Value];
        return [];
    }

    public async Task<RemoveUserViewModel?> GetRemoveUserViewModelAsync(
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

                return new RemoveUserViewModel(
                    OrganisationName: org.Name,
                    OrganisationSlug: org.Slug,
                    UserDisplayName: displayName,
                    Email: invite.Email,
                    CurrentRole: invite.OrganisationRole,
                    MemberSinceFormatted: invite.CreatedAt.ToString("d MMMM yyyy"),
                    CdpPersonId: null,
                    PendingInviteId: pendingInviteId.Value);
            }

            if (cdpPersonId.HasValue)
            {
                var user = await apiClient.Users2Async(org.CdpOrganisationGuid, cdpPersonId.Value, ct);
                if (user == null) return null;

                var displayName = !string.IsNullOrWhiteSpace(user.FirstName) && !string.IsNullOrWhiteSpace(user.LastName)
                    ? $"{user.FirstName} {user.LastName}"
                    : string.Empty;

                return new RemoveUserViewModel(
                    OrganisationName: org.Name,
                    OrganisationSlug: org.Slug,
                    UserDisplayName: displayName,
                    Email: user.Email ?? string.Empty,
                    CurrentRole: user.OrganisationRole,
                    MemberSinceFormatted: user.CreatedAt.ToString("d MMMM yyyy"),
                    CdpPersonId: cdpPersonId.Value,
                    PendingInviteId: null);
            }

            return null;
        }
        catch (ApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<bool> RemoveUserAsync(
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
                await apiClient.InvitesDELETEAsync(org.CdpOrganisationGuid, pendingInviteId.Value, ct);
                return true;
            }

            if (cdpPersonId.HasValue)
            {
                // TODO: delete user when endpoint is available. In the meantime, we can only delete pending invites, not existing users.
                return true;
            }

            return false;
        }
        catch (ApiClient.ApiException)
        {
            return false;
        }
    }

        public async Task<UserDetailsViewModel?> GetUserDetailsViewModelAsync(
        string organisationSlug,
        Guid cdpPersonId,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            var user = await apiClient.Users2Async(org.CdpOrganisationGuid, cdpPersonId, ct);
            var fullName = !string.IsNullOrWhiteSpace(user.FirstName) && !string.IsNullOrWhiteSpace(user.LastName)
                ? $"{user.FirstName} {user.LastName}"
                : string.Empty;
            var memberSince = user.JoinedAt.HasValue
                ? user.JoinedAt.Value.ToString("dd MMMM yyyy")
                : "Not available";

            var applicationAccess = user.ApplicationAssignments?
                .Where(assignment => assignment.Application != null)
                .Select(assignment => new UserApplicationAccessDetailViewModel(
                    ApplicationId: assignment.Application!.Id,
                    ApplicationName: assignment.Application!.Name,
                    ApplicationDescription: assignment.Application!.Description,
                    Permissions: assignment.Roles?.FirstOrDefault()?.Permissions?.Select(permission => permission.Name).ToList() ?? [],
                    AssignedDate: assignment.AssignedAt ?? DateTime.MinValue,
                    AssignedByEmail: assignment.AssignedBy ?? string.Empty,
                    ApplicationRole: assignment.Roles!.FirstOrDefault()!.Name))
                .ToList() ?? [];

            return new UserDetailsViewModel(
                Organisation: org,
                CdpPersonId: user.CdpPersonId ?? cdpPersonId,
                FullName: fullName,
                Email: user.Email ?? string.Empty,
                OrganisationRole: user.OrganisationRole,
                MemberSince: memberSince,
                ApplicationAccess: applicationAccess);
        }
        catch (ApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }
}
