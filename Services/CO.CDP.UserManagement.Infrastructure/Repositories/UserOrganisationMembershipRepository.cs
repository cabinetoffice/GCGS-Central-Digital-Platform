using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Data;
using CO.CDP.UserManagement.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.UserManagement.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for UserOrganisationMembership entities.
/// </summary>
public class UserOrganisationMembershipRepository(UserManagementDbContext context)
    : Repository<UserOrganisationMembership>(context), IUserOrganisationMembershipRepository
{
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
            .Where(m => m.OrganisationId == organisationId && m.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserOrganisationMembership?> GetByPersonIdAndOrganisationAsync(Guid cdpPersonId, int organisationId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(m => m.Organisation)
            .FirstOrDefaultAsync(m => m.CdpPersonId == cdpPersonId && m.OrganisationId == organisationId, cancellationToken);
    }

    public async Task<bool> ExistsByPersonIdAndOrganisationAsync(Guid cdpPersonId, int organisationId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(m => m.CdpPersonId == cdpPersonId && m.OrganisationId == organisationId && m.IsActive, cancellationToken);
    }

    public async Task<UserOrganisationMembership?> GetWithOrganisationAndRoleAsync(int id, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(m => m.Organisation)
            .Include(m => m.OrganisationRoleEntity)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<OrganisationRoleEntity?> GetOrganisationRoleAsync(OrganisationRole role, CancellationToken cancellationToken = default) =>
        await Context.Set<OrganisationRoleEntity>()
            .FirstOrDefaultAsync(d => d.Id == (int)role, cancellationToken);
}
