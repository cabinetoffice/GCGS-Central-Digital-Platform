using CO.CDP.ApplicationRegistry.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.ApplicationRegistry.Persistence.Repositories;

public class DatabaseUserAssignmentRepository : IUserAssignmentRepository
{
    private readonly ApplicationRegistryContext _context;

    public DatabaseUserAssignmentRepository(ApplicationRegistryContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserApplicationAssignment>> GetAssignmentsAsync(Guid organisationId, Guid applicationId)
    {
        return await _context.UserApplicationAssignments
            .Include(a => a.RoleAssignments)
                .ThenInclude(ra => ra.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .Where(a => a.OrganisationId == organisationId
                && a.ApplicationId == applicationId
                && a.IsActive)
            .OrderBy(a => a.UserPrincipalId)
            .ToListAsync();
    }

    public async Task<UserApplicationAssignment?> GetAssignmentAsync(Guid organisationId, Guid applicationId, string userPrincipalId)
    {
        return await _context.UserApplicationAssignments
            .Include(a => a.RoleAssignments)
                .ThenInclude(ra => ra.Role)
            .FirstOrDefaultAsync(a => a.OrganisationId == organisationId
                && a.ApplicationId == applicationId
                && a.UserPrincipalId == userPrincipalId
                && a.IsActive);
    }

    public async Task<UserApplicationAssignment> CreateAssignmentAsync(UserApplicationAssignment assignment)
    {
        _context.UserApplicationAssignments.Add(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }

    public async Task UpdateAssignmentAsync(UserApplicationAssignment assignment)
    {
        _context.UserApplicationAssignments.Update(assignment);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAssignmentAsync(Guid organisationId, Guid applicationId, string userPrincipalId)
    {
        var assignment = await _context.UserApplicationAssignments
            .FirstOrDefaultAsync(a => a.OrganisationId == organisationId
                && a.ApplicationId == applicationId
                && a.UserPrincipalId == userPrincipalId
                && a.IsActive);

        if (assignment != null)
        {
            assignment.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}
