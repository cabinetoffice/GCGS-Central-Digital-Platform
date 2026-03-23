using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.UserManagement.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ApplicationRole entities.
/// </summary>
public class RoleRepository(UserManagementDbContext context) : Repository<ApplicationRole>(context), IRoleRepository
{
    public async Task<IEnumerable<ApplicationRole>> GetByApplicationIdAsync(int applicationId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Permissions)
            .Where(r => r.ApplicationId == applicationId)
            .ToListAsync(cancellationToken);
    }

    public async Task<ApplicationRole?> GetByNameAsync(int applicationId, string name, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.ApplicationId == applicationId && r.Name == name, cancellationToken);
    }

    public async Task<ApplicationRole?> GetByIdWithPermissionsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ApplicationRole>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        var idArray = ids.Distinct().ToArray();
        return await DbSet
            .Where(r => idArray.Contains(r.Id))
            .ToListAsync(cancellationToken);
    }
}
