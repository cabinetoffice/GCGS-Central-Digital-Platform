using CO.CDP.UserManagement.Core.Entities;

namespace CO.CDP.UserManagement.Core.Interfaces;

/// <summary>
/// Repository interface for Organisation entities.
/// </summary>
public interface IOrganisationRepository : IRepository<Organisation>
{
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
    /// Checks if a slug already exists.
    /// </summary>
    /// <param name="slug">The slug to check.</param>
    /// <param name="excludeId">Optional ID to exclude from the check (for updates).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the slug exists; otherwise, false.</returns>
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default);
}
