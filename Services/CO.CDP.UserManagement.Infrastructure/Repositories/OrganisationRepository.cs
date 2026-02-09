using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.UserManagement.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Organisation entities.
/// </summary>
public class OrganisationRepository : Repository<Organisation>, IOrganisationRepository
{
    public OrganisationRepository(ApplicationRegistryDbContext context) : base(context)
    {
    }

    public async Task<Organisation?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(o => o.Slug == slug, cancellationToken);
    }

    public async Task<Organisation?> GetByCdpGuidAsync(Guid cdpOrganisationGuid, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(o => o.CdpOrganisationGuid == cdpOrganisationGuid, cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(o => o.Slug == slug);

        if (excludeId.HasValue)
        {
            query = query.Where(o => o.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
