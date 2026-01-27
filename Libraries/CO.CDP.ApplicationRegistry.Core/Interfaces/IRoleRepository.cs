using CO.CDP.ApplicationRegistry.Core.Entities;

namespace CO.CDP.ApplicationRegistry.Core.Interfaces;

/// <summary>
/// Repository interface for ApplicationRole entities.
/// </summary>
public interface IRoleRepository : IRepository<ApplicationRole>
{
    /// <summary>
    /// Gets all roles for a specific application including their permissions.
    /// </summary>
    /// <param name="applicationId">The application identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of roles for the application.</returns>
    Task<IEnumerable<ApplicationRole>> GetByApplicationIdAsync(int applicationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role by name within an application including its permissions.
    /// </summary>
    /// <param name="applicationId">The application identifier.</param>
    /// <param name="name">The role name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The role if found; otherwise, null.</returns>
    Task<ApplicationRole?> GetByNameAsync(int applicationId, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role by its ID including its permissions.
    /// </summary>
    /// <param name="id">The role identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The role if found; otherwise, null.</returns>
    Task<ApplicationRole?> GetByIdWithPermissionsAsync(int id, CancellationToken cancellationToken = default);
}
