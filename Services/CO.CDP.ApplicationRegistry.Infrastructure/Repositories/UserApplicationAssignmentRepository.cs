using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.ApplicationRegistry.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.ApplicationRegistry.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for UserApplicationAssignment entities.
/// </summary>
public class UserApplicationAssignmentRepository : Repository<UserApplicationAssignment>, IUserApplicationAssignmentRepository
{
    public UserApplicationAssignmentRepository(ApplicationRegistryDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<UserApplicationAssignment>> GetByMembershipIdAsync(int userOrganisationMembershipId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(ua => ua.OrganisationApplication)
                .ThenInclude(oa => oa.Application)
            .Include(ua => ua.Roles)
                .ThenInclude(r => r.Permissions)
            .Where(ua => ua.UserOrganisationMembershipId == userOrganisationMembershipId && ua.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserApplicationAssignment?> GetByMembershipAndApplicationAsync(int userOrganisationMembershipId, int organisationApplicationId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(ua => ua.OrganisationApplication)
                .ThenInclude(oa => oa.Application)
            .Include(ua => ua.Roles)
                .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(ua => ua.UserOrganisationMembershipId == userOrganisationMembershipId &&
                                       ua.OrganisationApplicationId == organisationApplicationId, cancellationToken);
    }

    public async Task<IEnumerable<UserApplicationAssignment>> GetAssignmentsForClaimsAsync(string userPrincipalId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(ua => ua.UserOrganisationMembership)
                .ThenInclude(m => m.Organisation)
            .Include(ua => ua.OrganisationApplication)
                .ThenInclude(oa => oa.Application)
            .Include(ua => ua.Roles)
                .ThenInclude(r => r.Permissions)
            .Where(ua => ua.UserOrganisationMembership.UserPrincipalId == userPrincipalId &&
                         ua.IsActive &&
                         ua.UserOrganisationMembership.IsActive &&
                         ua.OrganisationApplication.IsActive)
            .ToListAsync(cancellationToken);
    }
}
