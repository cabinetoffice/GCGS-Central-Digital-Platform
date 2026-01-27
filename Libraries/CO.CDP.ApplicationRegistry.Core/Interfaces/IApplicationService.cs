using CO.CDP.ApplicationRegistry.Core.Entities;

namespace CO.CDP.ApplicationRegistry.Core.Interfaces;

/// <summary>
/// Service for managing applications.
/// </summary>
public interface IApplicationService
{
    /// <summary>
    /// Gets an application by its identifier.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The application if found; otherwise, null.</returns>
    Task<Application?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an application by its client identifier.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The application if found; otherwise, null.</returns>
    Task<Application?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all applications.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of all applications.</returns>
    Task<IEnumerable<Application>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new application.
    /// </summary>
    /// <param name="name">The application name.</param>
    /// <param name="clientId">The unique client identifier.</param>
    /// <param name="description">The application description.</param>
    /// <param name="isActive">Whether the application is active.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created application.</returns>
    Task<Application> CreateAsync(string name, string clientId, string? description, bool isActive, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing application.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="name">The new name.</param>
    /// <param name="description">The new description.</param>
    /// <param name="isActive">The new active status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated application.</returns>
    Task<Application> UpdateAsync(int id, string name, string? description, bool isActive, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an application (soft delete).
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
