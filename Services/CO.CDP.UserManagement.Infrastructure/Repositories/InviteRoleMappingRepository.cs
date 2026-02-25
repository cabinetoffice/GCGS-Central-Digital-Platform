using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.UserManagement.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for InviteRoleMapping entities.
/// </summary>
public class InviteRoleMappingRepository : Repository<InviteRoleMapping>, IInviteRoleMappingRepository
{
    public InviteRoleMappingRepository(UserManagementDbContext context) : base(context)
    {
    }

    public async Task<InviteRoleMapping?> GetByCdpPersonInviteGuidAsync(Guid cdpPersonInviteGuid, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(i => i.Organisation)
            .Include(i => i.ApplicationAssignments)
                .ThenInclude(a => a.OrganisationApplication)
            .Include(i => i.ApplicationAssignments)
                .ThenInclude(a => a.ApplicationRole)
            .FirstOrDefaultAsync(i => i.CdpPersonInviteGuid == cdpPersonInviteGuid, cancellationToken);
    }

    public async Task<IEnumerable<InviteRoleMapping>> GetByOrganisationIdAsync(int organisationId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(i => i.Organisation)
            .Include(i => i.ApplicationAssignments)
                .ThenInclude(a => a.OrganisationApplication)
            .Include(i => i.ApplicationAssignments)
                .ThenInclude(a => a.ApplicationRole)
            .Where(i => i.OrganisationId == organisationId)
            .ToListAsync(cancellationToken);
    }
}
