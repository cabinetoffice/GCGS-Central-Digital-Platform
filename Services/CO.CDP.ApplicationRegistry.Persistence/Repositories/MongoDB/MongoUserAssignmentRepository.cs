using CO.CDP.ApplicationRegistry.Persistence.Entities;
using CO.CDP.ApplicationRegistry.Persistence.MongoDB;
using MongoDB.Driver;

namespace CO.CDP.ApplicationRegistry.Persistence.Repositories.MongoDB;

/// <summary>
/// MongoDB concrete implementation of <see cref="IUserAssignmentRepository"/>.
///
/// Document model:
///   <c>app_registry_user_assignments</c> — each document contains a flat <c>roleIds[]</c>
///   array (Guid references, not embedded role documents). Role names and permissions are
///   resolved at read time by fetching the corresponding <c>app_registry_applications</c>
///   document.
///
/// This hydration pattern satisfies the navigation chain required by
/// <c>GetClaimsTreeUseCase</c>:
///   assignment.RoleAssignments → ra.Role.RolePermissions → rp.Permission.Name
/// — all resolved inside this repository, transparent to callers.
/// </summary>
public class MongoUserAssignmentRepository : IUserAssignmentRepository
{
    private readonly IMongoCollection<UserApplicationAssignment> _assignments;
    private readonly IMongoCollection<Application>               _applications;
    private readonly IAuditRepository                            _audit;

    public MongoUserAssignmentRepository(MongoAppRegistryDatabase db, IAuditRepository audit)
    {
        _assignments  = db.UserAssignments;
        _applications = db.Applications;
        _audit        = audit;
    }

    // ── Read operations ────────────────────────────────────────────────────

    public async Task<IEnumerable<UserApplicationAssignment>> GetAssignmentsAsync(
        Guid organisationId, Guid applicationId)
    {
        var docs = await _assignments
            .Find(a => a.OrganisationId == organisationId
                    && a.ApplicationId  == applicationId
                    && a.IsActive)
            .SortBy(a => a.UserPrincipalId)
            .ToListAsync();

        if (docs.Count == 0) return [];

        var application = await _applications
            .Find(a => a.Id == applicationId)
            .FirstOrDefaultAsync();

        foreach (var doc in docs)
            HydrateRoleAssignments(doc, application);

        return docs;
    }

    public async Task<UserApplicationAssignment?> GetAssignmentAsync(
        Guid organisationId, Guid applicationId, string userPrincipalId)
    {
        var doc = await _assignments
            .Find(a => a.OrganisationId   == organisationId
                    && a.ApplicationId    == applicationId
                    && a.UserPrincipalId  == userPrincipalId
                    && a.IsActive)
            .FirstOrDefaultAsync();

        if (doc == null) return null;

        var application = await _applications
            .Find(a => a.Id == applicationId)
            .FirstOrDefaultAsync();

        HydrateRoleAssignments(doc, application);
        return doc;
    }

    // ── Write operations ───────────────────────────────────────────────────

    public async Task<UserApplicationAssignment> CreateAssignmentAsync(
        UserApplicationAssignment assignment)
    {
        assignment.AssignedAt = DateTimeOffset.UtcNow;
        await _assignments.InsertOneAsync(assignment);
        await _audit.LogAsync(new AuditLog
        {
            EntityType = nameof(UserApplicationAssignment),
            EntityId   = assignment.Id,
            Action     = "Created",
            UserId     = "system"
        });
        return assignment;
    }

    public async Task UpdateAssignmentAsync(UserApplicationAssignment assignment)
    {
        await _assignments.ReplaceOneAsync(a => a.Id == assignment.Id, assignment);
        await _audit.LogAsync(new AuditLog
        {
            EntityType = nameof(UserApplicationAssignment),
            EntityId   = assignment.Id,
            Action     = "Updated",
            UserId     = "system"
        });
    }

    public async Task RevokeAssignmentAsync(
        Guid organisationId, Guid applicationId, string userPrincipalId)
    {
        var update = Builders<UserApplicationAssignment>.Update
            .Set(a => a.IsActive, false);

        var result = await _assignments.UpdateOneAsync(
            a => a.OrganisationId  == organisationId
              && a.ApplicationId   == applicationId
              && a.UserPrincipalId == userPrincipalId
              && a.IsActive,
            update);

        if (result.ModifiedCount > 0)
        {
            var doc = await _assignments
                .Find(a => a.OrganisationId  == organisationId
                        && a.ApplicationId   == applicationId
                        && a.UserPrincipalId == userPrincipalId)
                .FirstOrDefaultAsync();

            if (doc != null)
                await _audit.LogAsync(new AuditLog
                {
                    EntityType = nameof(UserApplicationAssignment),
                    EntityId   = doc.Id,
                    Action     = "Revoked",
                    UserId     = "system"
                });
        }
    }

    // ── Private helpers ────────────────────────────────────────────────────

    /// <summary>
    /// Populates <see cref="UserApplicationAssignment.RoleAssignments"/> from the
    /// assignment's stored <c>roleIds</c> by resolving each ID against the embedded
    /// roles in the <paramref name="application"/> document, including each role's
    /// permissions. Satisfies the navigation chain expected by
    /// <c>GetClaimsTreeUseCase</c>.
    /// </summary>
    private static void HydrateRoleAssignments(
        UserApplicationAssignment assignment,
        Application? application)
    {
        if (application == null)
        {
            assignment.RoleAssignments = [];
            return;
        }

        // Build a lookup of roleId → ApplicationRole (with permissions hydrated).
        var roleMap = application.Roles.ToDictionary(r => r.Id);
        var permMap = application.Permissions.ToDictionary(p => p.Id);

        // Ensure each role has its RolePermissions list populated.
        foreach (var role in application.Roles)
        {
            foreach (var rp in role.RolePermissions)
            {
                if (rp.Permission == null && permMap.TryGetValue(rp.PermissionId, out var perm))
                    rp.Permission = perm;
            }
        }

        // UserApplicationAssignment stores roleIds as the RoleAssignments collection
        // (the Id of each UserRoleAssignment carries the assignment's own Id + the role Id).
        assignment.RoleAssignments = assignment.RoleAssignments
            .Where(ra => roleMap.ContainsKey(ra.RoleId))
            .Select(ra =>
            {
                ra.Role                         = roleMap[ra.RoleId];
                ra.UserApplicationAssignment    = assignment;
                return ra;
            })
            .ToList();
    }
}
