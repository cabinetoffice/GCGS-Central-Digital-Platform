using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.Functional;

namespace CO.CDP.UserManagement.App.Application.OrganisationRoles;

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

    Task<ChangeRoleState> GetOrCreateStateAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        ChangeUserRoleViewModel viewModel,
        CancellationToken ct);

    Task<ChangeRoleState?> GetValidatedStateAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        CancellationToken ct);

    ChangeUserRoleViewModel StateToViewModel(ChangeRoleState state);

    Task<ChangeUserRoleSuccessViewModel?> GetSuccessViewModelAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        CancellationToken ct);

    Task<OrganisationRoleChangeResult> ValidateAndSaveRoleChangeAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        ChangeUserRoleViewModel viewModel,
        OrganisationRole? selectedRole,
        CancellationToken ct);
}
