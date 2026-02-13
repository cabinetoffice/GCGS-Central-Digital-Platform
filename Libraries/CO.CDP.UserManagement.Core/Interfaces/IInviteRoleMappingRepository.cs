using CO.CDP.UserManagement.Core.Entities;

namespace CO.CDP.UserManagement.Core.Interfaces;

/// <summary>
/// Repository interface for InviteRoleMapping entities.
/// </summary>
public interface IInviteRoleMappingRepository : IRepository<InviteRoleMapping>
{
    /// <summary>
    /// Gets an invite role mapping by CDP person invite GUID.
    /// </summary>
    /// <param name="cdpPersonInviteGuid">The CDP person invite GUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The mapping if found; otherwise, null.</returns>
    Task<InviteRoleMapping?> GetByCdpPersonInviteGuidAsync(Guid cdpPersonInviteGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all invite role mappings for a specific organisation.
    /// </summary>
    /// <param name="organisationId">The organisation identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of invite role mappings.</returns>
    Task<IEnumerable<InviteRoleMapping>> GetByOrganisationIdAsync(int organisationId, CancellationToken cancellationToken = default);
}
