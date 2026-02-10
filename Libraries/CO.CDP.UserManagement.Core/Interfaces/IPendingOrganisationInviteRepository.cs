using CO.CDP.UserManagement.Core.Entities;

namespace CO.CDP.UserManagement.Core.Interfaces;

/// <summary>
/// Repository interface for PendingOrganisationInvite entities.
/// </summary>
public interface IPendingOrganisationInviteRepository : IRepository<PendingOrganisationInvite>
{
    /// <summary>
    /// Gets all pending invites for a specific organisation.
    /// </summary>
    /// <param name="organisationId">The organisation identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of pending invites.</returns>
    Task<IEnumerable<PendingOrganisationInvite>> GetByOrganisationIdAsync(int organisationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a pending invite by CDP person invite GUID.
    /// </summary>
    /// <param name="cdpPersonInviteGuid">The CDP person invite GUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The invite if found; otherwise, null.</returns>
    Task<PendingOrganisationInvite?> GetByCdpPersonInviteGuidAsync(Guid cdpPersonInviteGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a pending invite by email and organisation.
    /// </summary>
    /// <param name="email">The invited email address.</param>
    /// <param name="organisationId">The organisation identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The invite if found; otherwise, null.</returns>
    Task<PendingOrganisationInvite?> GetByEmailAndOrganisationAsync(string email, int organisationId, CancellationToken cancellationToken = default);
}
