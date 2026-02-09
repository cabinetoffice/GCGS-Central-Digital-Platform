using CO.CDP.UserManagement.Core.Entities;

namespace CO.CDP.UserManagement.Core.Interfaces;

/// <summary>
/// Repository interface for ApplicationPermission entities.
/// </summary>
public interface IPermissionRepository : IRepository<ApplicationPermission>
{
    /// <summary>
    /// Gets all permissions for a specific application.
    /// </summary>
    /// <param name="applicationId">The application identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of permissions for the application.</returns>
    Task<IEnumerable<ApplicationPermission>> GetByApplicationIdAsync(int applicationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a permission by name within an application.
    /// </summary>
    /// <param name="applicationId">The application identifier.</param>
    /// <param name="name">The permission name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The permission if found; otherwise, null.</returns>
    Task<ApplicationPermission?> GetByNameAsync(int applicationId, string name, CancellationToken cancellationToken = default);
}
