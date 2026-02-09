using CO.CDP.UserManagement.Core.Entities;

namespace CO.CDP.UserManagement.Core.Interfaces;

/// <summary>
/// Repository interface for UserOrganisationMembership entities.
/// </summary>
public interface IUserOrganisationMembershipRepository : IRepository<UserOrganisationMembership>
{
    /// <summary>
    /// Gets all memberships for a specific user.
    /// </summary>
    /// <param name="userPrincipalId">The user principal identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of user's organisation memberships.</returns>
    Task<IEnumerable<UserOrganisationMembership>> GetByUserPrincipalIdAsync(string userPrincipalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific user's membership in an organisation.
    /// </summary>
    /// <param name="userPrincipalId">The user principal identifier.</param>
    /// <param name="organisationId">The organisation identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The membership if found; otherwise, null.</returns>
    Task<UserOrganisationMembership?> GetByUserAndOrganisationAsync(string userPrincipalId, int organisationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all memberships for a specific organisation.
    /// </summary>
    /// <param name="organisationId">The organisation identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of memberships for the organisation.</returns>
    Task<IEnumerable<UserOrganisationMembership>> GetByOrganisationIdAsync(int organisationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific user's membership in an organisation by CDP person ID.
    /// </summary>
    /// <param name="cdpPersonId">The CDP person identifier.</param>
    /// <param name="organisationId">The organisation identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The membership if found; otherwise, null.</returns>
    Task<UserOrganisationMembership?> GetByPersonIdAndOrganisationAsync(Guid cdpPersonId, int organisationId, CancellationToken cancellationToken = default);
}
