using System;
using System.Threading;
using System.Threading.Tasks;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Services;

namespace CO.CDP.UserManagement.App.Application.OrganisationRoles
{
    public interface IOrganisationRoleFlowService
    {
        Task<ChangeUserRoleViewModel?> GetUserViewModelAsync(
            string organisationSlug,
            Guid cdpPersonId,
            CancellationToken ct);

        Task<ChangeUserRoleViewModel?> GetInviteViewModelAsync(
            string organisationSlug,
            Guid inviteGuid,
            CancellationToken ct);

        Task<ChangeUserRolePageViewModel> BuildPageViewModelAsync(
            ChangeUserRoleViewModel viewModel,
            OrganisationRole? selectedRole,
            CancellationToken ct);

        Task<Result<ServiceFailure, ServiceOutcome>> UpdateUserRoleAsync(
            string organisationSlug,
            Guid cdpPersonId,
            OrganisationRole role,
            CancellationToken ct);

        Task<Result<ServiceFailure, ServiceOutcome>> UpdateInviteRoleAsync(
            string organisationSlug,
            Guid inviteGuid,
            OrganisationRole role,
            CancellationToken ct);
        Task<bool> IsOwnerOrAdminAsync(
            string organisationSlug,
            string userId,
            CancellationToken ct);
    }
}
