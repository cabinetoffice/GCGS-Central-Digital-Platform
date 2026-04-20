using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.Functional;

namespace CO.CDP.UserManagement.App.Application.ApplicationRoles;

public interface IApplicationRoleFlowService
{
    Task<ChangeUserApplicationRolesViewModel?> GetUserViewModelAsync(
        Guid id,
        Guid cdpPersonId,
        CancellationToken ct);

    Task<ChangeUserApplicationRolesViewModel?> GetInviteViewModelAsync(
        Guid id,
        Guid inviteGuid,
        CancellationToken ct);

    Task<ChangeUserApplicationRolesViewModel?> GetUserViewModelWithStateAsync(
        Guid id,
        Guid cdpPersonId,
        CancellationToken ct);

    Task<ChangeUserApplicationRolesViewModel?> GetInviteViewModelWithStateAsync(
        Guid id,
        Guid inviteGuid,
        CancellationToken ct);

    ChangeApplicationRolesCheckViewModel BuildCheckViewModel(
        ChangeApplicationRoleState state);

    ChangeApplicationRolesSuccessViewModel? BuildSuccessViewModel(
        Guid id,
        ChangeApplicationRoleState state);

    IReadOnlyList<ApplicationRoleAssignmentPostModel> BuildAssignments(
        ChangeApplicationRoleState state);

    Task<Result<ServiceFailure, ServiceOutcome>> UpdateUserRolesAsync(
        Guid id,
        Guid cdpPersonId,
        IReadOnlyList<ApplicationRoleAssignmentPostModel> assignments,
        CancellationToken ct);

    Task<Result<ServiceFailure, ServiceOutcome>> UpdateInviteRolesAsync(
        Guid id,
        Guid inviteGuid,
        IReadOnlyList<ApplicationRoleAssignmentPostModel> assignments,
        CancellationToken ct);

    Task<ChangeApplicationRoleState?> GetValidatedStateAsync(
        Guid id,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        CancellationToken ct);

    Task ClearStateAsync(CancellationToken ct = default);

    Task<ApplicationRoleSubmitResult> ProcessSubmitAsync(
        Guid id,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        ApplicationRoleChangePostModel input,
        CancellationToken ct);
}
