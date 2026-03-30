using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Adapters;
using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;

namespace CO.CDP.UserManagement.App.Application.ApplicationRoles.Implementations;

public class ApplicationRoleFlowService : IApplicationRoleFlowService
{
    private readonly IUserManagementApiAdapter _adapter;

    public ApplicationRoleFlowService(IUserManagementApiAdapter adapter)
    {
        _adapter = adapter;
    }

    public async Task<ChangeUserApplicationRolesViewModel?> GetUserViewModelAsync(string organisationSlug,
        Guid cdpPersonId, CancellationToken ct)
    {
        var org = await _adapter.GetOrganisationBySlugAsync(organisationSlug, ct);
        if (org is null) return null;

        var user = await _adapter.GetUserAsync(org.CdpOrganisationGuid, cdpPersonId, ct);
        if (user is null) return null;

        return await BuildViewModelAsync(organisationSlug, org, cdpPersonId, null,
            $"{user.FirstName} {user.LastName}", user.Email, user.ApplicationAssignments?.ToList(), ct);
    }

    public async Task<ChangeUserApplicationRolesViewModel?> GetInviteViewModelAsync(string organisationSlug,
        Guid inviteGuid, CancellationToken ct)
    {
        var org = await _adapter.GetOrganisationBySlugAsync(organisationSlug, ct);
        if (org is null) return null;

        var invite = await _adapter.GetInviteAsync(org.CdpOrganisationGuid, inviteGuid, ct);
        if (invite is null) return null;

        // Map invite application assignments to the same shape as user assignments
        var inviteAssignments = invite.ApplicationAssignments?.Select(a => new UserAssignmentResponse
        {
            Id = 0,
            UserOrganisationMembershipId = 0,
            OrganisationApplicationId = a.OrganisationApplicationId,
            ApplicationId = a.ApplicationId,
            Roles = new[]
            {
                new RoleResponse
                {
                    Id = a.ApplicationRoleId,
                    ApplicationId = a.ApplicationId ?? 0,
                    Name = string.Empty,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = string.Empty
                }
            },
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();

        return await BuildViewModelAsync(organisationSlug, org, null, inviteGuid,
            $"{invite.FirstName} {invite.LastName}", invite.Email, inviteAssignments, ct);
    }

    public ChangeApplicationRolesCheckViewModel BuildCheckViewModel(ChangeApplicationRoleState state)
    {
        BuildAssignments(state);
        var apps = state.Applications
            .Where(a => a.GiveAccess)
            .Select(a => new ChangedApplicationRoleViewModel
            {
                ApplicationName = a.ApplicationName,
                CurrentRoleName = a.CurrentRoleName,
                NewRoleName = !string.IsNullOrEmpty(a.SelectedRoleName)
                    ? a.SelectedRoleName
                    : string.Join(", ", (a.SelectedRoleIds ?? Enumerable.Empty<int>()).Select(id => id.ToString())),
                IsNewAssignment = a.HasExistingAccess == false
            })
            .ToList();

        return new ChangeApplicationRolesCheckViewModel
        {
            OrganisationSlug = state.OrganisationSlug,
            UserDisplayName = state.UserDisplayName,
            Email = state.Email,
            ChangedApplications = apps,
        };
    }

    public IReadOnlyList<ApplicationRoleAssignmentPostModel> BuildAssignments(ChangeApplicationRoleState state)
    {
        return ApplicationRoleAssignmentsFactory.Build(state);
    }

    public async Task<Result<ServiceFailure, ServiceOutcome>> UpdateUserRolesAsync(string organisationSlug,
        Guid cdpPersonId, IReadOnlyList<ApplicationRoleAssignmentPostModel> assignments, CancellationToken ct)
    {
        var org = await _adapter.GetOrganisationBySlugAsync(organisationSlug, ct);
        if (org is null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

        var mappedRequests = assignments.Select(a => new ApplicationRoleAssignmentRequest(
            OrganisationApplicationId: a.OrganisationApplicationId,
            ApplicationId: a.ApplicationId,
            GiveAccess: a.GiveAccess,
            SelectedRoleId: a.SelectedRoleId,
            SelectedRoleIds: a.SelectedRoleIds
        )).ToList();

        var assignmentsForApi = mappedRequests.Select(r => new ApplicationRoleAssignment
        {
            OrganisationApplicationId = r.OrganisationApplicationId,
            ApplicationId = r.ApplicationId,
            RoleIds = r.GiveAccess
                ? (r.SelectedRoleIds?.AsEnumerable() ?? (r.SelectedRoleId.HasValue
                    ? new[] { r.SelectedRoleId.Value }
                    : Enumerable.Empty<int>()))
                : Enumerable.Empty<int>()
        }).ToList();

        var request = new UpdateUserAssignmentsRequest { Assignments = assignmentsForApi };

        return await _adapter.UpdateUserApplicationRolesAsync(org.Id, cdpPersonId, request, ct);
    }

    public async Task<Result<ServiceFailure, ServiceOutcome>> UpdateInviteRolesAsync(string organisationSlug,
        Guid inviteGuid, IReadOnlyList<ApplicationRoleAssignmentPostModel> assignments, CancellationToken ct)
    {
        var org = await _adapter.GetOrganisationBySlugAsync(organisationSlug, ct);
        if (org is null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

        var mappedRequests = assignments.Select(a => new ApplicationRoleAssignmentRequest(
            OrganisationApplicationId: a.OrganisationApplicationId,
            ApplicationId: a.ApplicationId,
            GiveAccess: a.GiveAccess,
            SelectedRoleId: a.SelectedRoleId,
            SelectedRoleIds: a.SelectedRoleIds
        )).ToList();

        var assignmentsForApi = mappedRequests.Select(r => new ApplicationRoleAssignment
        {
            OrganisationApplicationId = r.OrganisationApplicationId,
            ApplicationId = r.ApplicationId,
            RoleIds = r.GiveAccess
                ? (r.SelectedRoleIds?.AsEnumerable() ?? (r.SelectedRoleId.HasValue
                    ? new[] { r.SelectedRoleId.Value }
                    : Enumerable.Empty<int>()))
                : Enumerable.Empty<int>()
        }).ToList();

        var request = new UpdateUserAssignmentsRequest { Assignments = assignmentsForApi };

        return await _adapter.UpdateInviteApplicationRolesAsync(org.CdpOrganisationGuid, inviteGuid, request, ct);
    }

    private async Task<ChangeUserApplicationRolesViewModel> BuildViewModelAsync(
        string organisationSlug,
        OrganisationResponse org,
        Guid? cdpPersonId, Guid? inviteGuid,
        string displayName, string? email,
        IReadOnlyList<UserAssignmentResponse>? existingRoles,
        CancellationToken ct)
    {
        var applications = await _adapter.GetApplicationsAsync(org.Id, ct);

        var roleTasks = applications
            .Select(app => _adapter.GetApplicationRolesAsync(org.Id, app.ApplicationId, ct))
            .ToList();
        await Task.WhenAll(roleTasks);

        // Build a lookup of existing role IDs per OrganisationApplicationId
        var existingByOrgAppId = (existingRoles ?? Enumerable.Empty<UserAssignmentResponse>())
            .GroupBy(r => r.OrganisationApplicationId)
            .ToDictionary(g => g.Key,
                g => g.SelectMany(r => r.Roles?.Select(role => role.Id) ?? Enumerable.Empty<int>()).ToList());

        var appViewModels = applications.Select((app, i) =>
        {
            existingByOrgAppId.TryGetValue(app.Id, out var existingRoleIds);
            existingRoleIds ??= new List<int>();

            var availableRoles = roleTasks[i].Result;

            // Consider an assignment with no explicit roles as existing access
            var hasExistingAccess = existingRoleIds.Count > 0 || (existingRoles ?? Enumerable.Empty<UserAssignmentResponse>()).Any(r => r.OrganisationApplicationId == app.Id);

            return new ApplicationRoleChangeViewModel
            {
                OrganisationApplicationId = app.Id,
                ApplicationId = app.ApplicationId,
                ApplicationName = app.Application?.Name ?? string.Empty,
                AllowsMultipleRoleAssignments = app.Application?.AllowsMultipleRoleAssignments ?? false,
                IsEnabledByDefault = app.Application?.IsEnabledByDefault ?? false,
                HasExistingAccess = hasExistingAccess,
                GiveAccess = hasExistingAccess,
                SelectedRoleId = existingRoleIds.Count == 1 ? existingRoleIds[0] : (existingRoleIds.Count == 0 && hasExistingAccess && (availableRoles.Count == 1) ? availableRoles.FirstOrDefault()?.Id : null),
                SelectedRoleIds = existingRoleIds,
                Roles = availableRoles
                    .Select(r => new ApplicationRoleOptionViewModel { Id = r.Id, Name = r.Name })
                    .ToList()
            };
        }).ToList();

        return new ChangeUserApplicationRolesViewModel
        {
            OrganisationSlug = organisationSlug,
            CdpPersonId = cdpPersonId,
            InviteGuid = inviteGuid,
            UserDisplayName = displayName,
            Email = email ?? string.Empty,
            Applications = appViewModels
        };
    }
}