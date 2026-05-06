using CO.CDP.ApplicationRegistry.Persistence.Entities;

namespace CO.CDP.ApplicationRegistry.Persistence.Repositories;

public interface IApplicationRepository
{
    Task<Application?> GetByIdAsync(Guid id);
    Task<IEnumerable<Application>> GetAllAsync();
    Task<Application> CreateAsync(Application application);
    Task UpdateAsync(Application application);
    Task<ApplicationPermission?> GetPermissionByIdAsync(Guid permissionId);
    Task<IEnumerable<ApplicationPermission>> GetPermissionsAsync(Guid applicationId);
    Task<ApplicationPermission> CreatePermissionAsync(ApplicationPermission permission);
    Task UpdatePermissionAsync(ApplicationPermission permission);
    Task DeletePermissionAsync(Guid permissionId);
    Task<ApplicationRole?> GetRoleByIdAsync(Guid roleId);
    Task<IEnumerable<ApplicationRole>> GetRolesAsync(Guid applicationId);
    Task<ApplicationRole> CreateRoleAsync(ApplicationRole role);
    Task UpdateRoleAsync(ApplicationRole role);
    Task DeleteRoleAsync(Guid roleId);
    Task SetRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds);
}
