using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Adapters;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Requests;

namespace CO.CDP.UserManagement.App.Application.InviteUsers.Implementations
{
    public class InviteUserFlowService : IInviteUserFlowService
    {
        private readonly IUserManagementApiAdapter _adapter;
        private readonly IOrganisationRoleService _organisationRoleService;

        public InviteUserFlowService(IUserManagementApiAdapter adapter,
            IOrganisationRoleService organisationRoleService)
        {
            _adapter = adapter;
            _organisationRoleService = organisationRoleService;
        }

        public async Task<InviteUserViewModel?> GetViewModelAsync(Guid id,
            InviteUserViewModel? input, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationByGuidAsync(id, ct);
            if (org is null) return null;

            return new InviteUserViewModel
            {
                OrganisationId = id,
                OrganisationName = org.Name,
                Email = input?.Email,
                FirstName = input?.FirstName,
                LastName = input?.LastName,
                OrganisationRole = input?.OrganisationRole
            };
        }

        public async Task<bool> IsEmailAlreadyInOrganisationAsync(Guid id, string email,
            CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationByGuidAsync(id, ct);
            if (org is null) return false;

            var usersTask = _adapter.GetUsersAsync(org.CdpOrganisationGuid, ct);
            var invitesTask = _adapter.GetInvitesAsync(org.CdpOrganisationGuid, ct);
            await Task.WhenAll(usersTask, invitesTask);

            var users = usersTask.Result;
            var invites = invitesTask.Result;

            return users.Any(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase))
                   || invites.Any(i => string.Equals(i.Email, email, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<ApplicationRolesStepViewModel?> GetApplicationRolesStepAsync(Guid id,
            InviteUserState state, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationByGuidAsync(id, ct);
            if (org is null) return null;

            var applications = await _adapter.GetApplicationsAsync(org.Id, ct);

            var roleTasks = (applications)
                .Select(app => _adapter.GetApplicationRolesAsync(org.Id, app.ApplicationId, ct))
                .ToList();
            await Task.WhenAll(roleTasks);

            var appViewModels = (applications)
                .Select((app, i) => new ApplicationAccessSelectionViewModel
                {
                    OrganisationApplicationId = app.Id,
                    ApplicationName = app.Application?.Name ?? string.Empty,
                    AllowsMultipleRoleAssignments = app.Application?.AllowsMultipleRoleAssignments ?? false,
                    IsEnabledByDefault = app.Application?.IsEnabledByDefault ?? false,
                    Roles = roleTasks[i].Result
                        .Select(r => new ApplicationRoleOptionViewModel { Id = r.Id, Name = r.Name }).ToList()
                })
                .ToList();

            return new ApplicationRolesStepViewModel
            {
                OrganisationId = id,
                FirstName = state.FirstName,
                LastName = state.LastName,
                Email = state.Email,
                OrganisationRole = state.OrganisationRole,
                Applications = appViewModels
            };
        }

        public async Task<InviteCheckAnswersViewModel?> GetCheckAnswersViewModelAsync(Guid id,
            InviteUserState state, CancellationToken ct)
        {
            var rolesVm = await GetApplicationRolesStepAsync(id, state, ct);
            if (rolesVm is null) return null;

            var applications = (state.ApplicationAssignments ?? Enumerable.Empty<InviteApplicationAssignment>())
                .Select(assignment =>
                {
                    var app = rolesVm.Applications.FirstOrDefault(a =>
                        a.OrganisationApplicationId == assignment.OrganisationApplicationId);
                    var role = app?.Roles.FirstOrDefault(r => r.Id == assignment.ApplicationRoleId);
                    return app is null || role is null
                        ? null
                        : new InviteCheckAnswersApplicationViewModel
                        {
                            ApplicationName = app.ApplicationName,
                            RoleName = role.Name
                        };
                })
                .Where(x => x is not null)
                .Cast<InviteCheckAnswersApplicationViewModel>()
                .ToList();

            return new InviteCheckAnswersViewModel
            {
                OrganisationId = state.OrganisationId,
                FirstName = state.FirstName,
                LastName = state.LastName,
                Email = state.Email,
                OrganisationRole = state.OrganisationRole,
                Applications = applications
            };
        }

        public async Task<Result<ServiceFailure, ServiceOutcome>> InviteAsync(
            Guid id, InviteUserState state, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationByGuidAsync(id, ct);
            if (org is null)
                return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

            var request = new InviteUserRequest
            {
                Email = state.Email,
                FirstName = state.FirstName,
                LastName = state.LastName,
                OrganisationRole = state.OrganisationRole,
                ApplicationAssignments = (state.ApplicationAssignments ?? [])
                    .Select(a => new ApplicationAssignment
                    {
                        OrganisationApplicationId = a.OrganisationApplicationId,
                        ApplicationRoleIds = a.ApplicationRoleIds is { Count: > 0 }
                            ? a.ApplicationRoleIds.ToList()
                            : a.ApplicationRoleId > 0
                                ? [a.ApplicationRoleId]
                                : []
                    })
                    .ToList()
            };

            return await _adapter.InviteUserAsync(org.CdpOrganisationGuid, request, ct);
        }


        public async Task<Result<ServiceFailure, ResendInviteResult>> ResendInviteAsync(Guid id,
            Guid inviteGuid, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationByGuidAsync(id, ct);
            if (org is null)
                return Result<ServiceFailure, ResendInviteResult>.Success(
                    new ResendInviteResult(ServiceOutcome.NotFound));

            var invite = await _adapter.GetInviteAsync(org.CdpOrganisationGuid, inviteGuid, ct);
            var inviteeName = invite is null ? null : $"{invite.FirstName} {invite.LastName}".Trim();

            var result = await _adapter.ResendInviteAsync(org.CdpOrganisationGuid, inviteGuid, ct);
            return result.Match<Result<ServiceFailure, ResendInviteResult>>(
                Result<ServiceFailure, ResendInviteResult>.Failure,
                outcome =>
                    Result<ServiceFailure, ResendInviteResult>.Success(new ResendInviteResult(outcome, inviteeName)));
        }

        public async Task<OrganisationRoleStepViewModel> GetOrganisationRoleStepViewModelAsync(
            InviteUserState state, bool returnToCheckAnswers, CancellationToken ct)
        {
            return new OrganisationRoleStepViewModel(
                state.OrganisationId,
                state.FirstName,
                state.LastName,
                state.Email,
                state.OrganisationRole,
                returnToCheckAnswers,
                (await _organisationRoleService.GetRolesAsync(ct)).ToOptions());
        }
    }
}