using CO.CDP.ApplicationRegistry.Core.Entities;
using CO.CDP.ApplicationRegistry.Core.Interfaces;
using CO.CDP.ApplicationRegistry.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.ApplicationRegistry.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Application entities.
/// </summary>
public class ApplicationRepository : Repository<Application>, IApplicationRepository
{
    public ApplicationRepository(ApplicationRegistryDbContext context) : base(context)
    {
    }

    public async Task<Application?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(a => a.ClientId == clientId, cancellationToken);
    }

    public async Task<bool> ClientIdExistsAsync(string clientId, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(a => a.ClientId == clientId);

        if (excludeId.HasValue)
        {
            query = query.Where(a => a.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
