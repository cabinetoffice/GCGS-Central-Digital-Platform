using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;

namespace CO.CDP.UserManagement.App.Adapters;

public interface IUserManagementApiAdapter
{
    Task<OrganisationResponse?> GetOrganisationBySlugAsync(
        string organisationSlug, CancellationToken ct);

    Task<ICollection<OrganisationUserResponse>> GetUsersAsync(
        Guid organisationId, CancellationToken ct);

    Task<ICollection<PendingOrganisationInviteResponse>> GetInvitesAsync(
        Guid organisationId, CancellationToken ct);

    Task<ICollection<OrganisationApplicationResponse>> GetApplicationsAsync(
        int organisationId, CancellationToken ct);

    Task<ICollection<RoleResponse>> GetApplicationRolesAsync(
        int organisationId, int applicationId, CancellationToken ct);

    Task<ICollection<OrganisationRoleDefinitionResponse>> GetOrganisationRolesAsync(
        CancellationToken ct);

    Task<OrganisationUserResponse?> GetUserAsync(
        Guid organisationId, Guid cdpPersonId, CancellationToken ct);

    Task<PendingOrganisationInviteResponse?> GetInviteAsync(
        Guid organisationId, Guid inviteGuid, CancellationToken ct);

    Task<Result<ServiceFailure, ServiceOutcome>> InviteUserAsync(
        Guid organisationId, InviteUserRequest request, CancellationToken ct);

    Task<Result<ServiceFailure, ServiceOutcome>> ResendInviteAsync(
        Guid organisationId, Guid inviteGuid, CancellationToken ct);

    Task<Result<ServiceFailure, ServiceOutcome>> UpdateUserOrganisationRoleAsync(
        Guid organisationId, Guid cdpPersonId,
        ChangeOrganisationRoleRequest request, CancellationToken ct);

    Task<Result<ServiceFailure, ServiceOutcome>> UpdateInviteOrganisationRoleAsync(
        Guid organisationId, Guid inviteGuid,
        ChangeOrganisationRoleRequest request, CancellationToken ct);

    Task<Result<ServiceFailure, ServiceOutcome>> UpdateUserApplicationRolesAsync(
        int organisationId, Guid cdpPersonId,
        UpdateUserAssignmentsRequest request, CancellationToken ct);

    Task<Result<ServiceFailure, ServiceOutcome>> UpdateInviteApplicationRolesAsync(
        Guid cdpOrganisationId, Guid inviteGuid,
        UpdateUserAssignmentsRequest request, CancellationToken ct);

    Task<ICollection<UserAssignmentResponse>> GetUserAssignmentsAsync(
        int organisationId, Guid cdpPersonId, CancellationToken ct);

    Task<Result<ServiceFailure, ServiceOutcome>> DeleteUserAssignmentAsync(
        int organisationId, Guid cdpPersonId, int assignmentId, CancellationToken ct);

    Task<Result<ServiceFailure, ServiceOutcome>> RemoveUserAsync(
        Guid organisationId, Guid cdpPersonId, CancellationToken ct);

    Task<Result<ServiceFailure, ServiceOutcome>> CancelInviteAsync(
        Guid organisationId, int pendingInviteId, CancellationToken ct);
}