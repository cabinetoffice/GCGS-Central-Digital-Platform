using CO.CDP.ApplicationRegistry.Core.Entities;

namespace CO.CDP.ApplicationRegistry.Core.Interfaces;

/// <summary>
/// Repository interface for Application entities.
/// </summary>
public interface IApplicationRepository : IRepository<Application>
{
    /// <summary>
    /// Gets an application by its client identifier.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The application if found; otherwise, null.</returns>
    Task<Application?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a client ID already exists.
    /// </summary>
    /// <param name="clientId">The client ID to check.</param>
    /// <param name="excludeId">Optional ID to exclude from the check (for updates).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the client ID exists; otherwise, false.</returns>
    Task<bool> ClientIdExistsAsync(string clientId, int? excludeId = null, CancellationToken cancellationToken = default);
}
