using CO.CDP.ApplicationRegistry.Core.Entities;

namespace CO.CDP.ApplicationRegistry.Core.Interfaces;

/// <summary>
/// Service for managing application permissions.
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Gets a permission by its identifier.
    /// </summary>
    /// <param name="id">The permission identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The permission if found; otherwise, null.</returns>
    Task<ApplicationPermission?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permissions for a specific application.
    /// </summary>
    /// <param name="applicationId">The application identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of permissions for the application.</returns>
    Task<IEnumerable<ApplicationPermission>> GetByApplicationIdAsync(int applicationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new permission for an application.
    /// </summary>
    /// <param name="applicationId">The application identifier.</param>
    /// <param name="name">The permission name.</param>
    /// <param name="description">The permission description.</param>
    /// <param name="isActive">Whether the permission is active.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created permission.</returns>
    Task<ApplicationPermission> CreateAsync(int applicationId, string name, string? description, bool isActive, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing permission.
    /// </summary>
    /// <param name="id">The permission identifier.</param>
    /// <param name="name">The new name.</param>
    /// <param name="description">The new description.</param>
    /// <param name="isActive">The new active status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated permission.</returns>
    Task<ApplicationPermission> UpdateAsync(int id, string name, string? description, bool isActive, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a permission (soft delete).
    /// </summary>
    /// <param name="id">The permission identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
