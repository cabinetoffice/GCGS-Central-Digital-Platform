using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.UserManagement.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ApplicationPermission entities.
/// </summary>
public class PermissionRepository : Repository<ApplicationPermission>, IPermissionRepository
{
    public PermissionRepository(UserManagementDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ApplicationPermission>> GetByApplicationIdAsync(int applicationId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.ApplicationId == applicationId)
            .ToListAsync(cancellationToken);
    }

    public async Task<ApplicationPermission?> GetByNameAsync(int applicationId, string name, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(p => p.ApplicationId == applicationId && p.Name == name, cancellationToken);
    }
}
