using CO.CDP.ApplicationRegistry.Core.Entities;

namespace CO.CDP.ApplicationRegistry.Core.Interfaces;

/// <summary>
/// Service for managing application roles and their permissions.
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Gets a role by its identifier including permissions.
    /// </summary>
    /// <param name="id">The role identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The role if found; otherwise, null.</returns>
    Task<ApplicationRole?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all roles for a specific application including permissions.
    /// </summary>
    /// <param name="applicationId">The application identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of roles for the application.</returns>
    Task<IEnumerable<ApplicationRole>> GetByApplicationIdAsync(int applicationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new role for an application.
    /// </summary>
    /// <param name="applicationId">The application identifier.</param>
    /// <param name="name">The role name.</param>
    /// <param name="description">The role description.</param>
    /// <param name="isActive">Whether the role is active.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created role.</returns>
    Task<ApplicationRole> CreateAsync(int applicationId, string name, string? description, bool isActive, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing role.
    /// </summary>
    /// <param name="id">The role identifier.</param>
    /// <param name="name">The new name.</param>
    /// <param name="description">The new description.</param>
    /// <param name="isActive">The new active status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated role.</returns>
    Task<ApplicationRole> UpdateAsync(int id, string name, string? description, bool isActive, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a role (soft delete).
    /// </summary>
    /// <param name="id">The role identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns permissions to a role.
    /// </summary>
    /// <param name="roleId">The role identifier.</param>
    /// <param name="permissionIds">The collection of permission identifiers to assign.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated role with permissions.</returns>
    Task<ApplicationRole> AssignPermissionsAsync(int roleId, IEnumerable<int> permissionIds, CancellationToken cancellationToken = default);
}
