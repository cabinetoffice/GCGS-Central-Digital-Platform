using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.Core.Interfaces;

/// <summary>
/// Atomic bidirectional sync between UM and OI databases.
/// All operations run within a single PostgreSQL transaction via <c>IAtomicScope</c>
/// — both contexts commit or roll back together.
///
/// Strangler fig: new cross-DB membership operations go here.
/// Methods migrate in from <c>ICdpMembershipSyncService</c> (UM→OI) over time.
/// </summary>
public interface IAtomicMembershipSync
{
    /// <summary>
    /// Soft-removes a user from an organisation across both databases atomically.
    /// UM: sets membership IsActive=false, IsDeleted=true, DeletedAt/DeletedBy;
    ///     revokes all active application assignments.
    /// OI: removes UM-managed scopes from OrganisationPerson (preserves external scopes).
    /// Idempotent — returns successfully if membership is already inactive.
    /// </summary>
    /// <exception cref="CO.CDP.UserManagement.Core.Exceptions.EntityNotFoundException">
    /// Organisation or membership does not exist.
    /// </exception>
    /// <exception cref="CO.CDP.UserManagement.Core.Exceptions.LastOwnerRemovalException">
    /// Removing the last Owner from the organisation.
    /// </exception>
    Task RemoveUserFromOrganisationAsync(
        Guid cdpOrganisationId,
        Guid cdpPersonId,
        CancellationToken ct = default);

    /// <summary>
    /// Updates a user's organisation role across both databases atomically.
    /// UM: sets the new OrganisationRoleId on the membership.
    /// OI: upserts the OrganisationPerson scopes derived from the new role.
    /// </summary>
    /// <exception cref="CO.CDP.UserManagement.Core.Exceptions.EntityNotFoundException">
    /// Organisation or membership does not exist.
    /// </exception>
    Task<UserOrganisationMembership> UpdateMembershipRoleAsync(
        Guid cdpOrganisationId,
        Guid cdpPersonId,
        OrganisationRole newRole,
        CancellationToken ct = default);
}
