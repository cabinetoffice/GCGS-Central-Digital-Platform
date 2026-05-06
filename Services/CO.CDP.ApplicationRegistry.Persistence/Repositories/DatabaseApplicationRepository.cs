using CO.CDP.ApplicationRegistry.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.ApplicationRegistry.Persistence.Repositories;

public class DatabaseApplicationRepository : IApplicationRepository
{
    private readonly ApplicationRegistryContext _context;

    public DatabaseApplicationRepository(ApplicationRegistryContext context)
    {
        _context = context;
    }

    public async Task<Application?> GetByIdAsync(Guid id)
    {
        return await _context.Applications
            .Include(a => a.Permissions)
            .Include(a => a.Roles)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Application>> GetAllAsync()
    {
        return await _context.Applications
            .Where(a => a.IsActive)
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<Application> CreateAsync(Application application)
    {
        _context.Applications.Add(application);
        await _context.SaveChangesAsync();
        return application;
    }

    public async Task UpdateAsync(Application application)
    {
        application.UpdatedOn = DateTimeOffset.UtcNow;
        _context.Applications.Update(application);
        await _context.SaveChangesAsync();
    }

    public async Task<ApplicationPermission?> GetPermissionByIdAsync(Guid permissionId)
    {
        return await _context.ApplicationPermissions.FindAsync(permissionId);
    }

    public async Task<IEnumerable<ApplicationPermission>> GetPermissionsAsync(Guid applicationId)
    {
        return await _context.ApplicationPermissions
            .Where(p => p.ApplicationId == applicationId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<ApplicationPermission> CreatePermissionAsync(ApplicationPermission permission)
    {
        _context.ApplicationPermissions.Add(permission);
        await _context.SaveChangesAsync();
        return permission;
    }

    public async Task UpdatePermissionAsync(ApplicationPermission permission)
    {
        _context.ApplicationPermissions.Update(permission);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePermissionAsync(Guid permissionId)
    {
        var permission = await _context.ApplicationPermissions.FindAsync(permissionId);
        if (permission != null)
        {
            _context.ApplicationPermissions.Remove(permission);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<ApplicationRole?> GetRoleByIdAsync(Guid roleId)
    {
        return await _context.ApplicationRoles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == roleId);
    }

    public async Task<IEnumerable<ApplicationRole>> GetRolesAsync(Guid applicationId)
    {
        return await _context.ApplicationRoles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Where(r => r.ApplicationId == applicationId && r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<ApplicationRole> CreateRoleAsync(ApplicationRole role)
    {
        _context.ApplicationRoles.Add(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task UpdateRoleAsync(ApplicationRole role)
    {
        _context.ApplicationRoles.Update(role);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRoleAsync(Guid roleId)
    {
        var role = await _context.ApplicationRoles.FindAsync(roleId);
        if (role != null)
        {
            role.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }

    public async Task SetRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds)
    {
        var existing = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();

        _context.RolePermissions.RemoveRange(existing);

        foreach (var permissionId in permissionIds)
        {
            _context.RolePermissions.Add(new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            });
        }

        await _context.SaveChangesAsync();
    }
}
