using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.Functional;

namespace CO.CDP.UserManagement.App.Application.OrganisationRoles;

public interface IOrganisationRoleFlowService
{
    Task<ChangeUserRoleViewModel?> GetUserViewModelAsync(
        Guid id,
        Guid cdpPersonId,
        CancellationToken ct);

    Task<ChangeUserRoleViewModel?> GetInviteViewModelAsync(
        Guid id,
        Guid inviteGuid,
        CancellationToken ct);

    Task<ChangeUserRolePageViewModel> BuildPageViewModelAsync(
        ChangeUserRoleViewModel viewModel,
        OrganisationRole? selectedRole,
        CancellationToken ct);

    Task<Result<ServiceFailure, ServiceOutcome>> UpdateUserRoleAsync(
        Guid id,
        Guid cdpPersonId,
        OrganisationRole role,
        CancellationToken ct);

    Task<Result<ServiceFailure, ServiceOutcome>> UpdateInviteRoleAsync(
        Guid id,
        Guid inviteGuid,
        OrganisationRole role,
        CancellationToken ct);

    Task<ChangeRoleState> GetOrCreateStateAsync(
        Guid id,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        ChangeUserRoleViewModel viewModel,
        CancellationToken ct);

    Task<ChangeRoleState?> GetValidatedStateAsync(
        Guid id,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        CancellationToken ct);

    ChangeUserRoleViewModel StateToViewModel(ChangeRoleState state);

    Task<ChangeUserRoleSuccessViewModel?> GetSuccessViewModelAsync(
        Guid id,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        CancellationToken ct);

    Task<OrganisationRoleChangeResult> ValidateAndSaveRoleChangeAsync(
        Guid id,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        ChangeUserRoleViewModel viewModel,
        OrganisationRole? selectedRole,
        CancellationToken ct);
}
