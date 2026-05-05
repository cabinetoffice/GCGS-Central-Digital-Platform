using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;

namespace CO.CDP.UserManagement.App.Application.InviteUsers;

public interface IInviteUserFlowService
{
    Task<InviteUserViewModel?> GetViewModelAsync(
        Guid id,
        InviteUserViewModel? input,
        CancellationToken ct);

    Task<bool> IsEmailAlreadyInOrganisationAsync(
        Guid id,
        string email,
        CancellationToken ct);

    Task<ApplicationRolesStepViewModel?> GetApplicationRolesStepAsync(
        Guid id,
        InviteUserState state,
        CancellationToken ct);

    Task<InviteCheckAnswersViewModel?> GetCheckAnswersViewModelAsync(
        Guid id,
        InviteUserState state,
        CancellationToken ct);

    Task<Result<ServiceFailure, ServiceOutcome>> InviteAsync(
        Guid id,
        InviteUserState state,
        CancellationToken ct);

    Task<Result<ServiceFailure, ResendInviteResult>> ResendInviteAsync(
        Guid id,
        Guid inviteGuid,
        CancellationToken ct);

    Task<OrganisationRoleStepViewModel> GetOrganisationRoleStepViewModelAsync(
        InviteUserState state,
        bool returnToCheckAnswers,
        CancellationToken ct);
}