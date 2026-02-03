using CO.CDP.ApplicationRegistry.Core.Entities;

namespace CO.CDP.ApplicationRegistry.Core.Interfaces;

/// <summary>
/// Service interface for organisation user memberships.
/// </summary>
public interface IOrganisationUserService
{
    /// <summary>
    /// Gets all users in an organisation.
    /// </summary>
    /// <param name="organisationId">The organisation identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of organisation user memberships.</returns>
    Task<IEnumerable<UserOrganisationMembership>> GetOrganisationUsersAsync(
        int organisationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific user membership in an organisation.
    /// </summary>
    /// <param name="organisationId">The organisation identifier.</param>
    /// <param name="userPrincipalId">The user principal identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user membership if found.</returns>
    Task<UserOrganisationMembership?> GetOrganisationUserAsync(
        int organisationId,
        string userPrincipalId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific user membership in an organisation by CDP person ID.
    /// </summary>
    /// <param name="organisationId">The organisation identifier.</param>
    /// <param name="cdpPersonId">The CDP person identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user membership if found.</returns>
    Task<UserOrganisationMembership?> GetOrganisationUserByPersonIdAsync(
        int organisationId,
        Guid cdpPersonId,
        CancellationToken cancellationToken = default);
}
