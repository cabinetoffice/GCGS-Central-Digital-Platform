using CO.CDP.ApplicationRegistry.Core.Entities;
using CO.CDP.ApplicationRegistry.Core.Interfaces;
using CO.CDP.ApplicationRegistry.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.ApplicationRegistry.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ApplicationPermission entities.
/// </summary>
public class PermissionRepository : Repository<ApplicationPermission>, IPermissionRepository
{
    public PermissionRepository(ApplicationRegistryDbContext context) : base(context)
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
