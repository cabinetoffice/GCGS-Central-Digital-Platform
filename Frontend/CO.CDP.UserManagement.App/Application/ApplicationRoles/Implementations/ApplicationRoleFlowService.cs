using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Adapters;
using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using CO.CDP.UserManagement.Core.ApplicationRoles;

namespace CO.CDP.UserManagement.App.Application.ApplicationRoles.Implementations;

public class ApplicationRoleFlowService : IApplicationRoleFlowService
{
    private readonly IUserManagementApiAdapter _adapter;
    private readonly IChangeApplicationRoleStateStore _changeApplicationRoleStateStore;

    public ApplicationRoleFlowService(
        IUserManagementApiAdapter adapter,
        IChangeApplicationRoleStateStore changeApplicationRoleStateStore)
    {
        _adapter = adapter;
        _changeApplicationRoleStateStore = changeApplicationRoleStateStore;
    }

    public async Task<ChangeUserApplicationRolesViewModel?> GetUserViewModelAsync(Guid id,
        Guid cdpPersonId, CancellationToken ct)
    {
        var org = await _adapter.GetOrganisationByGuidAsync(id, ct);
        if (org is null) return null;

        var user = await _adapter.GetUserAsync(org.CdpOrganisationGuid, cdpPersonId, ct);
        if (user is null) return null;

        return await BuildViewModelAsync(id, org, cdpPersonId, null,
            $"{user.FirstName} {user.LastName}", user.Email, user.ApplicationAssignments?.ToList(), ct);
    }

    public async Task<ChangeUserApplicationRolesViewModel?> GetInviteViewModelAsync(Guid id,
        Guid inviteGuid, CancellationToken ct)
    {
        var org = await _adapter.GetOrganisationByGuidAsync(id, ct);
        if (org is null) return null;

        var invite = await _adapter.GetInviteAsync(org.CdpOrganisationGuid, inviteGuid, ct);
        if (invite is null) return null;

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

        return await BuildViewModelAsync(id, org, null, inviteGuid,
            $"{invite.FirstName} {invite.LastName}", invite.Email, inviteAssignments, ct);
    }

    public async Task<ChangeUserApplicationRolesViewModel?> GetUserViewModelWithStateAsync(
        Guid id, Guid cdpPersonId, CancellationToken ct)
    {
        var viewModel = await GetUserViewModelAsync(id, cdpPersonId, ct);
        if (viewModel is null) return null;

        var state = await GetValidatedStateAsync(id, cdpPersonId, null, ct);
        if (state is not null) MergeState(viewModel, state);

        return viewModel;
    }

    public async Task<ChangeUserApplicationRolesViewModel?> GetInviteViewModelWithStateAsync(
        Guid id, Guid inviteGuid, CancellationToken ct)
    {
        var viewModel = await GetInviteViewModelAsync(id, inviteGuid, ct);
        if (viewModel is null) return null;

        var state = await GetValidatedStateAsync(id, null, inviteGuid, ct);
        if (state is not null) MergeState(viewModel, state);

        return viewModel;
    }

    public ChangeApplicationRolesCheckViewModel BuildCheckViewModel(ChangeApplicationRoleState state) =>
        new()
        {
            OrganisationId = state.OrganisationId,
            UserDisplayName = state.UserDisplayName,
            Email = state.Email,
            IsPending = state.InviteGuid.HasValue,
            CdpPersonId = state.CdpPersonId,
            InviteGuid = state.InviteGuid,
            ChangedApplications = state.Applications
                .Where(a => (!a.HasExistingAccess && a.GiveAccess) || (a.HasExistingAccess && HasRoleChanged(a)))
                .Select(a => new ChangedApplicationRoleViewModel
                {
                    ApplicationName = a.ApplicationName,
                    CurrentRoleName = a.CurrentRoleName,
                    NewRoleName = a.SelectedRoleName,
                    IsNewAssignment = !a.HasExistingAccess
                })
                .ToList()
        };

    public ChangeApplicationRolesSuccessViewModel? BuildSuccessViewModel(
        Guid id, ChangeApplicationRoleState state)
    {
        var changedApplications = state.Applications
            .Where(a => (!a.HasExistingAccess && a.GiveAccess) || (a.HasExistingAccess && HasRoleChanged(a)))
            .Select(a => new ChangedApplicationRoleViewModel
            {
                ApplicationName = a.ApplicationName,
                CurrentRoleName = a.CurrentRoleName,
                NewRoleName = a.SelectedRoleName,
                IsNewAssignment = !a.HasExistingAccess
            })
            .ToList();

        if (changedApplications.Count == 0) return null;

        return new ChangeApplicationRolesSuccessViewModel
        {
            OrganisationId = id,
            UserDisplayName = state.UserDisplayName,
            ChangedApplications = changedApplications
        };
    }

    public IReadOnlyList<ApplicationRoleAssignmentPostModel> BuildAssignments(ChangeApplicationRoleState state)
    {
        return ApplicationRoleAssignmentsFactory.Build(state);
    }

    public async Task<Result<ServiceFailure, ServiceOutcome>> UpdateUserRolesAsync(Guid id,
        Guid cdpPersonId, IReadOnlyList<ApplicationRoleAssignmentPostModel> assignments, CancellationToken ct)
    {
        var org = await _adapter.GetOrganisationByGuidAsync(id, ct);
        if (org is null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

        var request = BuildUpdateRequest(assignments);
        return await _adapter.UpdateUserApplicationRolesAsync(org.Id, cdpPersonId, request, ct);
    }

    public async Task<Result<ServiceFailure, ServiceOutcome>> UpdateInviteRolesAsync(Guid id,
        Guid inviteGuid, IReadOnlyList<ApplicationRoleAssignmentPostModel> assignments, CancellationToken ct)
    {
        var org = await _adapter.GetOrganisationByGuidAsync(id, ct);
        if (org is null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

        var request = BuildUpdateRequest(assignments);
        return await _adapter.UpdateInviteApplicationRolesAsync(org.CdpOrganisationGuid, inviteGuid, request, ct);
    }

    public async Task<ChangeApplicationRoleState?> GetValidatedStateAsync(
        Guid id, Guid? cdpPersonId, Guid? inviteGuid, CancellationToken ct)
    {
        var state = await _changeApplicationRoleStateStore.GetAsync();
        if (state is null) return null;

        if (state.OrganisationId != id ||
            state.CdpPersonId != cdpPersonId ||
            state.InviteGuid != inviteGuid)
        {
            await _changeApplicationRoleStateStore.ClearAsync();
            return null;
        }

        return state;
    }

    public async Task ClearStateAsync(CancellationToken ct = default) =>
        await _changeApplicationRoleStateStore.ClearAsync();

    public async Task<ApplicationRoleSubmitResult> ProcessSubmitAsync(
        Guid id, Guid? cdpPersonId, Guid? inviteGuid,
        ApplicationRoleChangePostModel input, CancellationToken ct)
    {
        ChangeUserApplicationRolesViewModel? viewModel = cdpPersonId.HasValue
            ? await GetUserViewModelAsync(id, cdpPersonId.Value, ct)
            : await GetInviteViewModelAsync(id, inviteGuid!.Value, ct);

        if (viewModel is null) return new ApplicationRoleSubmitResult.NotFound();

        var plannerInput = ApplicationRoleChangePlannerMapper.Map(
            viewModel, input, id, cdpPersonId, inviteGuid);
        var planResult = ApplicationRoleChangePlanner.Plan(plannerInput);

        if (!planResult.IsValid)
            return new ApplicationRoleSubmitResult.ValidationError(viewModel, planResult.Errors);

        var assignmentStates = planResult.Output!.Assignments
            .Select(a => new ApplicationRoleAssignmentState(
                a.OrganisationApplicationId,
                a.ApplicationId,
                a.ApplicationName,
                a.HasExistingAccess,
                a.GiveAccess,
                a.CurrentSingleRoleId,
                a.CurrentRoleName,
                a.SelectedSingleRoleId,
                a.SelectedRoleName,
                SelectedRoleIds: a.SelectedRoleIds.ToList(),
                CurrentRoleIds: a.CurrentRoleIds?.ToList()))
            .ToList();

        var state = new ChangeApplicationRoleState(
            id, cdpPersonId, inviteGuid,
            viewModel.UserDisplayName, viewModel.Email, assignmentStates);

        await _changeApplicationRoleStateStore.SetAsync(state);
        return new ApplicationRoleSubmitResult.Saved();
    }

    private static void MergeState(ChangeUserApplicationRolesViewModel viewModel, ChangeApplicationRoleState state)
    {
        var stateByOrgAppId = state.Applications.ToDictionary(a => a.OrganisationApplicationId);
        foreach (var app in viewModel.Applications)
        {
            if (stateByOrgAppId.TryGetValue(app.OrganisationApplicationId, out var stateApp))
            {
                app.GiveAccess = stateApp.GiveAccess;
                app.SelectedRoleId = stateApp.SelectedRoleId;
            }
        }
    }

    private static bool HasRoleChanged(ApplicationRoleAssignmentState a)
    {
        var selected = (a.SelectedRoleIds ?? (a.SelectedRoleId.HasValue ? [a.SelectedRoleId.Value] : [])).OrderBy(x => x);
        var current = (a.CurrentRoleIds ?? (a.CurrentRoleId.HasValue ? [a.CurrentRoleId.Value] : new List<int>())).OrderBy(x => x);
        return !selected.SequenceEqual(current);
    }

    private static UpdateUserAssignmentsRequest BuildUpdateRequest(IReadOnlyList<ApplicationRoleAssignmentPostModel> assignments)
    {
        var mapped = assignments.Select(a => new ApplicationRoleAssignment
        {
            OrganisationApplicationId = a.OrganisationApplicationId,
            ApplicationId = a.ApplicationId,
            RoleIds = a.GiveAccess
                ? (a.SelectedRoleIds?.AsEnumerable() ?? (a.SelectedRoleId.HasValue
                    ? new[] { a.SelectedRoleId.Value }
                    : Enumerable.Empty<int>()))
                : Enumerable.Empty<int>()
        }).ToList();

        return new UpdateUserAssignmentsRequest { Assignments = mapped };
    }

    private async Task<ChangeUserApplicationRolesViewModel> BuildViewModelAsync(
        Guid id,
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

        var existingByOrgAppId = (existingRoles ?? Enumerable.Empty<UserAssignmentResponse>())
            .GroupBy(r => r.OrganisationApplicationId)
            .ToDictionary(g => g.Key,
                g => g.SelectMany(r => r.Roles?.Select(role => role.Id) ?? Enumerable.Empty<int>()).ToList());

        var appViewModels = applications.Select((app, i) =>
        {
            existingByOrgAppId.TryGetValue(app.Id, out var existingRoleIds);
            existingRoleIds ??= new List<int>();

            var availableRoles = roleTasks[i].Result;
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
            OrganisationId = id,
            CdpPersonId = cdpPersonId,
            InviteGuid = inviteGuid,
            UserDisplayName = displayName,
            Email = email ?? string.Empty,
            Applications = appViewModels
        };
    }
}
