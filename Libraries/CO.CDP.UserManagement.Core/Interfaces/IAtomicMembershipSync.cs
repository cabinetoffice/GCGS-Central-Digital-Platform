using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;

namespace CO.CDP.UserManagement.Core.Interfaces;

/// <summary>
/// Atomic bidirectional sync between UM and OI databases.
/// All operations run within a single PostgreSQL transaction via <c>IAtomicScope</c>
/// — both contexts commit or roll back together.
///
/// Strangler fig: every cross-DB membership/assignment operation lives here.
/// <c>ICdpMembershipSyncService</c> is fully replaced by this interface.
/// </summary>
public interface IAtomicMembershipSync
{
    /// <summary>
    /// Soft-removes a user from an organisation across both databases atomically.
    /// UM: sets membership IsActive=false, IsDeleted=true, DeletedAt/DeletedBy;
    ///     revokes all active application assignments.
    /// OI: removes UM-managed scopes from OrganisationPerson.
    /// Idempotent — returns successfully if membership is already inactive.
    /// </summary>
    Task RemoveUserFromOrganisationAsync(
        Guid cdpOrganisationId,
        Guid cdpPersonId,
        CancellationToken ct = default);

    /// <summary>
    /// Updates a user's organisation role across both databases atomically.
    /// UM: sets the new OrganisationRoleId on the membership.
    /// OI: upserts the OrganisationPerson scopes derived from the new role.
    /// </summary>
    Task<UserOrganisationMembership> UpdateMembershipRoleAsync(
        Guid cdpOrganisationId,
        Guid cdpPersonId,
        OrganisationRole newRole,
        CancellationToken ct = default);

    /// <summary>
    /// Assigns a user to an application within an organisation atomically.
    /// UM: creates (or reactivates) a <see cref="UserApplicationAssignment"/> with the given roles.
    /// OI: recomputes and upserts the OrganisationPerson scopes.
    /// </summary>
    Task<UserApplicationAssignment> AssignUserToApplicationAsync(
        string userId,
        int organisationId,
        int applicationId,
        IEnumerable<int> roleIds,
        CancellationToken ct = default);

    /// <summary>
    /// Updates a user's application assignment roles atomically.
    /// UM: replaces the roles on the assignment.
    /// OI: recomputes and upserts the OrganisationPerson scopes.
    /// </summary>
    Task<UserApplicationAssignment> UpdateApplicationAssignmentAsync(
        string userId,
        int organisationId,
        int assignmentId,
        IEnumerable<int> roleIds,
        CancellationToken ct = default);

    /// <summary>
    /// Revokes a user's application assignment atomically.
    /// UM: sets IsActive=false, RevokedAt/RevokedBy on the assignment.
    /// OI: recomputes and upserts the OrganisationPerson scopes.
    /// </summary>
    Task RevokeApplicationAssignmentAsync(
        string userId,
        int organisationId,
        int assignmentId,
        CancellationToken ct = default);

    /// <summary>
    /// Accepts an invite and creates a membership with default assignments atomically.
    /// UM: creates membership, removes invite mapping, assigns default applications.
    /// OI: upserts OrganisationPerson scopes.
    /// </summary>
    Task AcceptInviteAsync(
        Guid cdpOrganisationId,
        int inviteRoleMappingId,
        AcceptOrganisationInviteRequest request,
        CancellationToken ct = default);
}
