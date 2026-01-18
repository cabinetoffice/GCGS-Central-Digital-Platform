using CO.CDP.ApplicationRegistry.Core.Entities;

namespace CO.CDP.ApplicationRegistry.Core.Interfaces;

/// <summary>
/// Repository interface for OrganisationApplication entities.
/// </summary>
public interface IOrganisationApplicationRepository : IRepository<OrganisationApplication>
{
    /// <summary>
    /// Gets all applications enabled for a specific organisation.
    /// </summary>
    /// <param name="organisationId">The organisation identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of organisation applications.</returns>
    Task<IEnumerable<OrganisationApplication>> GetByOrganisationIdAsync(int organisationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific organisation-application relationship.
    /// </summary>
    /// <param name="organisationId">The organisation identifier.</param>
    /// <param name="applicationId">The application identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The organisation application if found; otherwise, null.</returns>
    Task<OrganisationApplication?> GetByOrganisationAndApplicationAsync(int organisationId, int applicationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all applications accessible by a specific user across all their organisations.
    /// </summary>
    /// <param name="userPrincipalId">The user principal identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of applications the user has access to.</returns>
    Task<IEnumerable<OrganisationApplication>> GetApplicationsByUserAsync(string userPrincipalId, CancellationToken cancellationToken = default);
}
