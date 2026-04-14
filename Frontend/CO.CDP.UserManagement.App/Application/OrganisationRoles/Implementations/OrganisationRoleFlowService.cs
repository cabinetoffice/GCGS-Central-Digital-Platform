using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Adapters;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Requests;

namespace CO.CDP.UserManagement.App.Application.OrganisationRoles.Implementations
{
    public class OrganisationRoleFlowService : IOrganisationRoleFlowService
    {
        private readonly IUserManagementApiAdapter _adapter;
        private readonly IOrganisationRoleService _organisationRoleService;
        public OrganisationRoleFlowService(IUserManagementApiAdapter adapter, IOrganisationRoleService organisationRoleService)
        {
            _adapter = adapter;
            _organisationRoleService = organisationRoleService;
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
    }
}
