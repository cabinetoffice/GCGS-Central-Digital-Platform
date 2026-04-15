using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.Functional;

namespace CO.CDP.UserManagement.App.Application.ApplicationRoles;

public interface IApplicationRoleFlowService
{
    Task<ChangeUserApplicationRolesViewModel?> GetUserViewModelAsync(
        string organisationSlug,
        Guid cdpPersonId,
        CancellationToken ct);

    Task<ChangeUserApplicationRolesViewModel?> GetInviteViewModelAsync(
        string organisationSlug,
        Guid inviteGuid,
        CancellationToken ct);

    Task<ChangeUserApplicationRolesViewModel?> GetUserViewModelWithStateAsync(
        string organisationSlug,
        Guid cdpPersonId,
        CancellationToken ct);

    Task<ChangeUserApplicationRolesViewModel?> GetInviteViewModelWithStateAsync(
        string organisationSlug,
        Guid inviteGuid,
        CancellationToken ct);

    ChangeApplicationRolesCheckViewModel BuildCheckViewModel(
        ChangeApplicationRoleState state);

    ChangeApplicationRolesSuccessViewModel? BuildSuccessViewModel(
        string organisationSlug,
        ChangeApplicationRoleState state);

    IReadOnlyList<ApplicationRoleAssignmentPostModel> BuildAssignments(
        ChangeApplicationRoleState state);

    Task<Result<ServiceFailure, ServiceOutcome>> UpdateUserRolesAsync(
        string organisationSlug,
        Guid cdpPersonId,
        IReadOnlyList<ApplicationRoleAssignmentPostModel> assignments,
        CancellationToken ct);

    Task<Result<ServiceFailure, ServiceOutcome>> UpdateInviteRolesAsync(
        string organisationSlug,
        Guid inviteGuid,
        IReadOnlyList<ApplicationRoleAssignmentPostModel> assignments,
        CancellationToken ct);

    Task<ChangeApplicationRoleState?> GetValidatedStateAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        CancellationToken ct);

    Task ClearStateAsync(CancellationToken ct = default);

    Task<ApplicationRoleSubmitResult> ProcessSubmitAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        ApplicationRoleChangePostModel input,
        CancellationToken ct);
}
