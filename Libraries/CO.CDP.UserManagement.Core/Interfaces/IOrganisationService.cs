using CO.CDP.UserManagement.Core.Entities;

namespace CO.CDP.UserManagement.Core.Interfaces;

/// <summary>
/// Service for managing organisations.
/// </summary>
public interface IOrganisationService
{
    /// <summary>
    /// Gets an organisation by its identifier.
    /// </summary>
    /// <param name="id">The organisation identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The organisation if found; otherwise, null.</returns>
    Task<Organisation?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an organisation by its slug.
    /// </summary>
    /// <param name="slug">The organisation slug.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The organisation if found; otherwise, null.</returns>
    Task<Organisation?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an organisation by its CDP organisation GUID.
    /// </summary>
    /// <param name="cdpOrganisationGuid">The CDP organisation GUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The organisation if found; otherwise, null.</returns>
    Task<Organisation?> GetByCdpGuidAsync(Guid cdpOrganisationGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all organisations.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of all organisations.</returns>
    Task<IEnumerable<Organisation>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new organisation.
    /// </summary>
    /// <param name="cdpOrganisationGuid">The CDP organisation GUID.</param>
    /// <param name="name">The organisation name.</param>
    /// <param name="isActive">Whether the organisation is active.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created organisation.</returns>
    Task<Organisation> CreateAsync(Guid cdpOrganisationGuid, string name, bool isActive, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing organisation.
    /// </summary>
    /// <param name="id">The organisation identifier.</param>
    /// <param name="name">The new name.</param>
    /// <param name="isActive">The new active status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated organisation.</returns>
    Task<Organisation> UpdateAsync(int id, string name, bool isActive, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an organisation (soft delete).
    /// </summary>
    /// <param name="id">The organisation identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
