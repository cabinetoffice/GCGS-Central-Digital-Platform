using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.UserManagement.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for PendingOrganisationInvite entities.
/// </summary>
public class PendingOrganisationInviteRepository : Repository<PendingOrganisationInvite>, IPendingOrganisationInviteRepository
{
    public PendingOrganisationInviteRepository(UserManagementDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PendingOrganisationInvite>> GetByOrganisationIdAsync(int organisationId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(i => i.OrganisationId == organisationId)
            .ToListAsync(cancellationToken);
    }

    public async Task<PendingOrganisationInvite?> GetByCdpPersonInviteGuidAsync(Guid cdpPersonInviteGuid, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(i => i.Organisation)
            .FirstOrDefaultAsync(i => i.CdpPersonInviteGuid == cdpPersonInviteGuid, cancellationToken);
    }

    public async Task<PendingOrganisationInvite?> GetByEmailAndOrganisationAsync(string email, int organisationId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(i => i.Email == email && i.OrganisationId == organisationId, cancellationToken);
    }
}
