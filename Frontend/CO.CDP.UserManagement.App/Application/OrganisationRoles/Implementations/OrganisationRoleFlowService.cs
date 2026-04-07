using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Adapters;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.OrganisationRoles;
using AppOrgRoleService = CO.CDP.UserManagement.App.Services.IOrganisationRoleService;

namespace CO.CDP.UserManagement.App.Application.OrganisationRoles.Implementations
{
    public class OrganisationRoleFlowService : IOrganisationRoleFlowService
    {
        private readonly IUserManagementApiAdapter _adapter;
        private readonly AppOrgRoleService _organisationRoleService;
        private readonly IChangeRoleStateStore _changeRoleStateStore;
        private readonly ICurrentUserService _currentUserService;

        public OrganisationRoleFlowService(
            IUserManagementApiAdapter adapter,
            AppOrgRoleService organisationRoleService,
            IChangeRoleStateStore changeRoleStateStore,
            ICurrentUserService currentUserService)
        {
            _adapter = adapter;
            _organisationRoleService = organisationRoleService;
            _changeRoleStateStore = changeRoleStateStore;
            _currentUserService = currentUserService;
        }

        public async Task<ChangeUserRoleViewModel?> GetUserViewModelAsync(string organisationSlug, Guid cdpPersonId, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationBySlugAsync(organisationSlug, ct);
            if (org is null) return null;

            var user = await _adapter.GetUserAsync(org.CdpOrganisationGuid, cdpPersonId, ct);
            if (user is null) return null;

            return new ChangeUserRoleViewModel(
                OrganisationName: org.Name,
                OrganisationSlug: organisationSlug,
                UserDisplayName: $"{user.FirstName} {user.LastName}",
                Email: user.Email,
                CurrentRole: user.OrganisationRole,
                SelectedRole: null,
                IsPending: false,
                CdpPersonId: cdpPersonId,
                InviteGuid: null
            );
        }

        public async Task<ChangeUserRoleViewModel?> GetInviteViewModelAsync(string organisationSlug, Guid inviteGuid, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationBySlugAsync(organisationSlug, ct);
            if (org is null) return null;

            var invite = await _adapter.GetInviteAsync(org.CdpOrganisationGuid, inviteGuid, ct);
            if (invite is null) return null;

            return new ChangeUserRoleViewModel(
                OrganisationName: org.Name,
                OrganisationSlug: organisationSlug,
                UserDisplayName: $"{invite.FirstName} {invite.LastName}",
                Email: invite.Email,
                CurrentRole: invite.OrganisationRole,
                SelectedRole: null,
                IsPending: true,
                CdpPersonId: null,
                InviteGuid: inviteGuid
            );
        }

        public async Task<ChangeUserRolePageViewModel> BuildPageViewModelAsync(ChangeUserRoleViewModel viewModel, OrganisationRole? selectedRole, CancellationToken ct)
        {
            var roles = await _organisationRoleService.GetRolesAsync(ct);
            return ChangeUserRolePageViewModel.From(viewModel, roles.ToOptions(), selectedRole);
        }

        public async Task<Result<ServiceFailure, ServiceOutcome>> UpdateUserRoleAsync(string organisationSlug, Guid cdpPersonId, OrganisationRole role, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationBySlugAsync(organisationSlug, ct);
            if (org is null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

            return await _adapter.UpdateUserOrganisationRoleAsync(org.CdpOrganisationGuid, cdpPersonId, new ChangeOrganisationRoleRequest { OrganisationRole = role }, ct);
        }

        public async Task<Result<ServiceFailure, ServiceOutcome>> UpdateInviteRoleAsync(string organisationSlug, Guid inviteGuid, OrganisationRole role, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationBySlugAsync(organisationSlug, ct);
            if (org is null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

            return await _adapter.UpdateInviteOrganisationRoleAsync(org.CdpOrganisationGuid, inviteGuid, new ChangeOrganisationRoleRequest { OrganisationRole = role }, ct);
        }

        public async Task<bool> IsOwnerOrAdminAsync(string organisationSlug, string userId, CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationBySlugAsync(organisationSlug, ct);
            if (org is null) return false;

            var users = await _adapter.GetUsersAsync(org.CdpOrganisationGuid, ct);
            return users.Any(u => u.CdpPersonId?.ToString() == userId && (u.OrganisationRole == OrganisationRole.Owner || u.OrganisationRole == OrganisationRole.Admin));
        }

        public async Task<ChangeRoleState> GetOrCreateStateAsync(
            string organisationSlug, Guid? cdpPersonId, Guid? inviteGuid,
            ChangeUserRoleViewModel viewModel, CancellationToken ct)
        {
            var existing = await GetValidatedStateAsync(organisationSlug, cdpPersonId, inviteGuid, ct);
            if (existing is not null) return existing;

            var selectedRole = viewModel.SelectedRole ?? viewModel.CurrentRole;
            var state = ToChangeRoleState(viewModel, selectedRole);
            await _changeRoleStateStore.SetAsync(state);
            return state;
        }

        public async Task<ChangeRoleState?> GetValidatedStateAsync(
            string organisationSlug, Guid? cdpPersonId, Guid? inviteGuid, CancellationToken ct)
        {
            var state = await _changeRoleStateStore.GetAsync();
            if (state is null) return null;

            if (!state.OrganisationSlug.Equals(organisationSlug, StringComparison.OrdinalIgnoreCase) ||
                state.CdpPersonId != cdpPersonId ||
                state.InviteGuid != inviteGuid)
            {
                await _changeRoleStateStore.ClearAsync();
                return null;
            }

            return state;
        }

        public ChangeUserRoleViewModel StateToViewModel(ChangeRoleState state) =>
            new(
                OrganisationName: string.Empty,
                OrganisationSlug: state.OrganisationSlug,
                UserDisplayName: state.UserDisplayName,
                Email: state.Email,
                CurrentRole: state.CurrentRole,
                SelectedRole: state.SelectedRole,
                IsPending: state.InviteGuid.HasValue,
                CdpPersonId: state.CdpPersonId,
                InviteGuid: state.InviteGuid);

        public async Task<ChangeUserRoleSuccessViewModel?> GetSuccessViewModelAsync(
            string organisationSlug, Guid? cdpPersonId, Guid? inviteGuid, CancellationToken ct)
        {
            var state = await GetValidatedStateAsync(organisationSlug, cdpPersonId, inviteGuid, ct);
            if (state is null) return null;

            await _changeRoleStateStore.ClearAsync();

            var roleDescription = (await _organisationRoleService.GetRoleAsync(state.SelectedRole, ct))?.Description
                                  ?? string.Empty;
            return new ChangeUserRoleSuccessViewModel(
                OrganisationSlug: organisationSlug,
                UserDisplayName: state.UserDisplayName,
                NewRole: state.SelectedRole,
                RoleDescription: roleDescription);
        }

        public async Task<OrganisationRoleChangeResult> ValidateAndSaveRoleChangeAsync(
            string organisationSlug,
            Guid? cdpPersonId,
            Guid? inviteGuid,
            ChangeUserRoleViewModel viewModel,
            OrganisationRole? selectedRole,
            CancellationToken ct)
        {
            var org = await _adapter.GetOrganisationBySlugAsync(organisationSlug, ct);
            if (org is null) return new OrganisationRoleChangeResult.NotFound();

            var currentUserEmail = _currentUserService.GetUserEmail();
            var currentUserOrgRole = ResolveCurrentUserOrgRole(org.CdpOrganisationGuid);

            var validation = OrganisationRoleChangeValidator.Validate(
                selectedRole,
                viewModel.CurrentRole,
                viewModel.Email,
                currentUserEmail,
                currentUserOrgRole);

            if (!validation.IsValid)
                return new OrganisationRoleChangeResult.ValidationError(validation.ModelKey!, validation.ErrorMessage!);

            var state = ToChangeRoleState(viewModel with { SelectedRole = selectedRole!.Value }, selectedRole!.Value);
            await _changeRoleStateStore.SetAsync(state);

            return new OrganisationRoleChangeResult.Saved();
        }

        private OrganisationRole? ResolveCurrentUserOrgRole(Guid orgId)
        {
            var orgClaim = _currentUserService.GetCdpClaims()?.Organisations
                .FirstOrDefault(o => o.OrganisationId == orgId);
            if (orgClaim is null) return null;
            return Enum.TryParse<OrganisationRole>(orgClaim.OrganisationRole, ignoreCase: true, out var parsed)
                ? parsed : null;
        }

        private static ChangeRoleState ToChangeRoleState(ChangeUserRoleViewModel viewModel, OrganisationRole selectedRole) =>
            new(
                viewModel.OrganisationSlug,
                viewModel.CdpPersonId,
                viewModel.InviteGuid,
                viewModel.UserDisplayName,
                viewModel.Email,
                viewModel.CurrentRole,
                selectedRole);
    }
}

