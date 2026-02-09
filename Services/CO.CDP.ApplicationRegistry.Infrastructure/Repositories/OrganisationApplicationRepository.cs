using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.ApplicationRegistry.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.ApplicationRegistry.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for OrganisationApplication entities.
/// </summary>
public class OrganisationApplicationRepository : Repository<OrganisationApplication>, IOrganisationApplicationRepository
{
    public OrganisationApplicationRepository(ApplicationRegistryDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<OrganisationApplication>> GetByOrganisationIdAsync(int organisationId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(oa => oa.Application)
            .Include(oa => oa.Organisation)
            .Where(oa => oa.OrganisationId == organisationId && oa.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<OrganisationApplication?> GetByOrganisationAndApplicationAsync(int organisationId, int applicationId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(oa => oa.Application)
            .Include(oa => oa.Organisation)
            .FirstOrDefaultAsync(oa => oa.OrganisationId == organisationId && oa.ApplicationId == applicationId, cancellationToken);
    }

    public async Task<IEnumerable<OrganisationApplication>> GetApplicationsByUserAsync(string userPrincipalId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(oa => oa.Application)
            .Include(oa => oa.Organisation)
            .Where(oa => oa.IsActive &&
                         oa.Organisation.UserMemberships.Any(m => m.UserPrincipalId == userPrincipalId && m.IsActive))
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
