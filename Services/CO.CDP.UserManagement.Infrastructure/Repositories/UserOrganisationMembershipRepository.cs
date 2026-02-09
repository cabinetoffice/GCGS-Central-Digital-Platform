using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.UserManagement.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for UserOrganisationMembership entities.
/// </summary>
public class UserOrganisationMembershipRepository : Repository<UserOrganisationMembership>, IUserOrganisationMembershipRepository
{
    public UserOrganisationMembershipRepository(ApplicationRegistryDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<UserOrganisationMembership>> GetByUserPrincipalIdAsync(string userPrincipalId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(m => m.Organisation)
            .Where(m => m.UserPrincipalId == userPrincipalId && m.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserOrganisationMembership?> GetByUserAndOrganisationAsync(string userPrincipalId, int organisationId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(m => m.Organisation)
            .FirstOrDefaultAsync(m => m.UserPrincipalId == userPrincipalId && m.OrganisationId == organisationId, cancellationToken);
    }

    public async Task<IEnumerable<UserOrganisationMembership>> GetByOrganisationIdAsync(int organisationId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(m => m.ApplicationAssignments)
            .Where(m => m.OrganisationId == organisationId && m.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserOrganisationMembership?> GetByPersonIdAndOrganisationAsync(Guid cdpPersonId, int organisationId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(m => m.Organisation)
            .FirstOrDefaultAsync(m => m.CdpPersonId == cdpPersonId && m.OrganisationId == organisationId, cancellationToken);
    }
}
