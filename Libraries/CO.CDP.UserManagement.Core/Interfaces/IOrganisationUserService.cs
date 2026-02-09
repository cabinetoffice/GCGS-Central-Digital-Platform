using CO.CDP.UserManagement.Core.Entities;

namespace CO.CDP.UserManagement.Core.Interfaces;

/// <summary>
/// Service interface for organisation user memberships.
/// </summary>
public interface IOrganisationUserService
{
    /// <summary>
    /// Gets all users in an organisation.
    /// </summary>
    /// <param name="cdpOrganisationId">The CDP organisation identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of organisation user memberships.</returns>
    Task<IEnumerable<UserOrganisationMembership>> GetOrganisationUsersAsync(
        Guid cdpOrganisationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific user membership in an organisation.
    /// </summary>
    /// <param name="cdpOrganisationId">The CDP organisation identifier.</param>
    /// <param name="userPrincipalId">The user principal identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user membership if found.</returns>
    Task<UserOrganisationMembership?> GetOrganisationUserAsync(
        Guid cdpOrganisationId,
        string userPrincipalId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific user membership in an organisation by CDP person ID.
    /// </summary>
    /// <param name="cdpOrganisationId">The CDP organisation identifier.</param>
    /// <param name="cdpPersonId">The CDP person identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user membership if found.</returns>
    Task<UserOrganisationMembership?> GetOrganisationUserByPersonIdAsync(
        Guid cdpOrganisationId,
        Guid cdpPersonId,
        CancellationToken cancellationToken = default);
}
