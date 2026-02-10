using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CoreEntities = CO.CDP.UserManagement.Core.Entities;

namespace CO.CDP.UserManagement.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Organisation entities.
/// </summary>
public class OrganisationRepository : Repository<CoreEntities.Organisation>, IOrganisationRepository
{
    public OrganisationRepository(UserManagementDbContext context) : base(context)
    {
    }

    public async Task<CoreEntities.Organisation?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(o => o.Slug == slug, cancellationToken);
    }

    public async Task<CoreEntities.Organisation?> GetByCdpGuidAsync(Guid cdpOrganisationGuid, CancellationToken cancellationToken = default)
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
