using CO.CDP.ApplicationRegistry.Core.Entities;

namespace CO.CDP.ApplicationRegistry.Core.Interfaces;

/// <summary>
/// Service for managing organisation-application relationships.
/// </summary>
public interface IOrganisationApplicationService
{
    /// <summary>
    /// Gets all applications enabled for an organisation.
    /// </summary>
    /// <param name="organisationId">The organisation identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of organisation applications.</returns>
    Task<IEnumerable<OrganisationApplication>> GetByOrganisationIdAsync(int organisationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all applications accessible by a user across all their organisations.
    /// </summary>
    /// <param name="userPrincipalId">The user principal identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of applications the user has access to.</returns>
    Task<IEnumerable<OrganisationApplication>> GetApplicationsByUserAsync(string userPrincipalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables an application for an organisation.
    /// </summary>
    /// <param name="organisationId">The organisation identifier.</param>
    /// <param name="applicationId">The application identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created organisation application relationship.</returns>
    Task<OrganisationApplication> EnableApplicationAsync(int organisationId, int applicationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disables an application for an organisation.
    /// </summary>
    /// <param name="organisationId">The organisation identifier.</param>
    /// <param name="applicationId">The application identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DisableApplicationAsync(int organisationId, int applicationId, CancellationToken cancellationToken = default);
}
