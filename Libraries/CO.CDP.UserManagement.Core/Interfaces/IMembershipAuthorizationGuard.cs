using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.Core.Interfaces;

/// <summary>
/// Pre-flight authorization guard for membership mutation operations.
/// Evaluates actor/target permission rules (self-service, role hierarchy) against
/// live repository state before the operation enters the atomic transaction.
/// Transactional invariants (e.g. last-owner protection) are enforced separately
/// inside <c>IAtomicMembershipSync</c>.
/// </summary>
public interface IMembershipAuthorizationGuard
{
    /// <summary>
    /// Validates that the current actor is permitted to remove the target user.
    /// Throws <see cref="CO.CDP.UserManagement.Core.Exceptions.MembershipOperationForbiddenException"/>
    /// if the actor is the target (self-removal) or if an Admin attempts to remove an Owner.
    /// </summary>
    Task ValidateRemovalAsync(Guid cdpOrganisationId, Guid cdpPersonId, CancellationToken ct = default);

    /// <summary>
    /// Validates that the current actor is permitted to change the target user's role.
    /// Throws <see cref="CO.CDP.UserManagement.Core.Exceptions.MembershipOperationForbiddenException"/>
    /// if the actor is the target (self-modification) or if an Admin attempts to modify an Owner's role.
    /// </summary>
    Task ValidateRoleChangeAsync(Guid cdpOrganisationId, Guid cdpPersonId, OrganisationRole newRole, CancellationToken ct = default);
}
