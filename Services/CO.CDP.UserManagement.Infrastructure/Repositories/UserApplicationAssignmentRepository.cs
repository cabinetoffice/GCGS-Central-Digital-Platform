using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.UserManagement.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for UserApplicationAssignment entities.
/// </summary>
public class UserApplicationAssignmentRepository(UserManagementDbContext context)
    : Repository<UserApplicationAssignment>(context), IUserApplicationAssignmentRepository
{
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

    public async Task<IEnumerable<UserApplicationAssignment>> GetByMembershipIdsAsync(IEnumerable<int> userOrganisationMembershipIds, CancellationToken cancellationToken = default)
    {
        var membershipIds = userOrganisationMembershipIds.ToArray();
        if (membershipIds.Length == 0)
        {
            return [];
        }

        return await DbSet
            .Include(ua => ua.OrganisationApplication)
                .ThenInclude(oa => oa.Application)
            .Include(ua => ua.Roles)
                .ThenInclude(r => r.Permissions)
            .Where(ua => membershipIds.Contains(ua.UserOrganisationMembershipId) && ua.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserApplicationAssignment?> GetByMembershipAndApplicationAsync(int userOrganisationMembershipId, int organisationApplicationId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(ua => ua.OrganisationApplication)
                .ThenInclude(oa => oa.Application)
            .Include(ua => ua.Roles)
                .ThenInclude(r => r.Permissions)
            .SingleOrDefaultAsync(ua => ua.UserOrganisationMembershipId == userOrganisationMembershipId &&
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

    public async Task<IEnumerable<UserApplicationAssignment>> GetActiveForSyncAsync(int membershipId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(a => a.Roles)
            .Where(a => a.UserOrganisationMembershipId == membershipId
                        && !a.IsDeleted
                        && a.IsActive
                        && a.UserOrganisationMembership.IsActive
                        && a.OrganisationApplication.IsActive)
            .ToListAsync(cancellationToken);
}
