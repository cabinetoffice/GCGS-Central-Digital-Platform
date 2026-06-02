using CO.CDP.ApplicationRegistry.Persistence.Entities;
using CO.CDP.ApplicationRegistry.Persistence.MongoDB;
using MongoDB.Driver;

namespace CO.CDP.ApplicationRegistry.Persistence.Repositories.MongoDB;

/// <summary>
/// MongoDB concrete implementation of <see cref="IApplicationRepository"/>.
///
/// Document model:
///   <c>app_registry_applications</c> — each document embeds its <c>roles[]</c>; each role
///   embeds <c>permissions[]</c>.  There is no separate RolePermission collection — the
///   relationship lives inside the application document.
///
///   Standalone permissions (not yet assigned to any role) are stored in the application's
///   top-level <c>permissions[]</c> array so they can be referenced when calling
///   <see cref="SetRolePermissionsAsync"/>.
/// </summary>
public class MongoApplicationRepository : IApplicationRepository
{
    private readonly IMongoCollection<Application> _applications;
    private readonly IAuditRepository _audit;

    public MongoApplicationRepository(MongoAppRegistryDatabase db, IAuditRepository audit)
    {
        _applications = db.Applications;
        _audit = audit;
    }

    // ── Read operations ────────────────────────────────────────────────────

    public async Task<Application?> GetByIdAsync(Guid id)
    {
        return await _applications
            .Find(a => a.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Application>> GetAllAsync()
    {
        return await _applications
            .Find(a => a.IsActive)
            .SortBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<ApplicationPermission?> GetPermissionByIdAsync(Guid permissionId)
    {
        // Permissions are embedded in the application document.
        // Project out the permissions array and find the matching entry.
        var application = await _applications
            .Find(a => a.Permissions.Any(p => p.Id == permissionId))
            .FirstOrDefaultAsync();

        return application?.Permissions.FirstOrDefault(p => p.Id == permissionId);
    }

    public async Task<IEnumerable<ApplicationPermission>> GetPermissionsAsync(Guid applicationId)
    {
        var application = await _applications
            .Find(a => a.Id == applicationId)
            .FirstOrDefaultAsync();

        return application?.Permissions
                   .OrderBy(p => p.Name)
                   .ToList()
               ?? [];
    }

    public async Task<ApplicationRole?> GetRoleByIdAsync(Guid roleId)
    {
        // Roles are embedded — find the application containing this role.
        var application = await _applications
            .Find(a => a.Roles.Any(r => r.Id == roleId))
            .FirstOrDefaultAsync();

        if (application == null) return null;

        var role = application.Roles.FirstOrDefault(r => r.Id == roleId);
        if (role == null) return null;

        // Hydrate RolePermissions from the application's standalone permissions.
        HydrateRolePermissions(role, application.Permissions);
        return role;
    }

    public async Task<IEnumerable<ApplicationRole>> GetRolesAsync(Guid applicationId)
    {
        var application = await _applications
            .Find(a => a.Id == applicationId)
            .FirstOrDefaultAsync();

        if (application == null) return [];

        var activeRoles = application.Roles
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToList();

        foreach (var role in activeRoles)
            HydrateRolePermissions(role, application.Permissions);

        return activeRoles;
    }

    // ── Write operations ───────────────────────────────────────────────────

    public async Task<Application> CreateAsync(Application application)
    {
        application.CreatedOn = DateTimeOffset.UtcNow;
        application.UpdatedOn = DateTimeOffset.UtcNow;
        await _applications.InsertOneAsync(application);
        await _audit.LogAsync(new AuditLog
        {
            EntityType = nameof(Application),
            EntityId   = application.Id,
            Action     = "Created",
            UserId     = "system"
        });
        return application;
    }

    public async Task UpdateAsync(Application application)
    {
        application.UpdatedOn = DateTimeOffset.UtcNow;
        await _applications.ReplaceOneAsync(a => a.Id == application.Id, application);
        await _audit.LogAsync(new AuditLog
        {
            EntityType = nameof(Application),
            EntityId   = application.Id,
            Action     = "Updated",
            UserId     = "system"
        });
    }

    public async Task<ApplicationPermission> CreatePermissionAsync(ApplicationPermission permission)
    {
        var update = Builders<Application>.Update
            .Push(a => a.Permissions, permission);

        await _applications.UpdateOneAsync(a => a.Id == permission.ApplicationId, update);
        await _audit.LogAsync(new AuditLog
        {
            EntityType = nameof(ApplicationPermission),
            EntityId   = permission.Id,
            Action     = "Created",
            UserId     = "system"
        });
        return permission;
    }

    public async Task UpdatePermissionAsync(ApplicationPermission permission)
    {
        // Replace the matching permission in the embedded array using arrayFilters.
        var filter = Builders<Application>.Filter.And(
            Builders<Application>.Filter.Eq(a => a.Id, permission.ApplicationId),
            Builders<Application>.Filter.ElemMatch(a => a.Permissions, p => p.Id == permission.Id));

        var update = Builders<Application>.Update
            .Set("permissions.$[perm].name",        permission.Name)
            .Set("permissions.$[perm].description", permission.Description);

        var options = new UpdateOptions
        {
            ArrayFilters = [new BsonDocumentArrayFilterDefinition<global::MongoDB.Bson.BsonDocument>(
                global::MongoDB.Bson.BsonDocument.Parse($"{{\"perm.id\": \"{permission.Id}\"}}"))]
        };

        await _applications.UpdateOneAsync(
            Builders<Application>.Filter.Eq(a => a.Id, permission.ApplicationId),
            update,
            options);

        await _audit.LogAsync(new AuditLog
        {
            EntityType = nameof(ApplicationPermission),
            EntityId   = permission.Id,
            Action     = "Updated",
            UserId     = "system"
        });
    }

    public async Task DeletePermissionAsync(Guid permissionId)
    {
        var application = await _applications
            .Find(a => a.Permissions.Any(p => p.Id == permissionId))
            .FirstOrDefaultAsync();

        if (application == null) return;

        var update = Builders<Application>.Update
            .PullFilter(a => a.Permissions, p => p.Id == permissionId);

        await _applications.UpdateOneAsync(a => a.Id == application.Id, update);
        await _audit.LogAsync(new AuditLog
        {
            EntityType = nameof(ApplicationPermission),
            EntityId   = permissionId,
            Action     = "Deleted",
            UserId     = "system"
        });
    }

    public async Task<ApplicationRole> CreateRoleAsync(ApplicationRole role)
    {
        var update = Builders<Application>.Update
            .Push(a => a.Roles, role);

        await _applications.UpdateOneAsync(a => a.Id == role.ApplicationId, update);
        await _audit.LogAsync(new AuditLog
        {
            EntityType = nameof(ApplicationRole),
            EntityId   = role.Id,
            Action     = "Created",
            UserId     = "system"
        });
        return role;
    }

    public async Task UpdateRoleAsync(ApplicationRole role)
    {
        var update = Builders<Application>.Update
            .Set("roles.$[r].name",        role.Name)
            .Set("roles.$[r].description", role.Description)
            .Set("roles.$[r].isActive",    role.IsActive);

        var options = new UpdateOptions
        {
            ArrayFilters = [new BsonDocumentArrayFilterDefinition<global::MongoDB.Bson.BsonDocument>(
                global::MongoDB.Bson.BsonDocument.Parse($"{{\"r.id\": \"{role.Id}\"}}"))]
        };

        var application = await _applications
            .Find(a => a.Roles.Any(r => r.Id == role.Id))
            .FirstOrDefaultAsync();

        if (application == null) return;

        await _applications.UpdateOneAsync(
            Builders<Application>.Filter.Eq(a => a.Id, application.Id),
            update,
            options);

        await _audit.LogAsync(new AuditLog
        {
            EntityType = nameof(ApplicationRole),
            EntityId   = role.Id,
            Action     = "Updated",
            UserId     = "system"
        });
    }

    public async Task DeleteRoleAsync(Guid roleId)
    {
        // Soft-delete: set isActive = false on the embedded role.
        var update = Builders<Application>.Update
            .Set("roles.$[r].isActive", false);

        var options = new UpdateOptions
        {
            ArrayFilters = [new BsonDocumentArrayFilterDefinition<global::MongoDB.Bson.BsonDocument>(
                global::MongoDB.Bson.BsonDocument.Parse($"{{\"r.id\": \"{roleId}\"}}"))]
        };

        var application = await _applications
            .Find(a => a.Roles.Any(r => r.Id == roleId))
            .FirstOrDefaultAsync();

        if (application == null) return;

        await _applications.UpdateOneAsync(
            Builders<Application>.Filter.Eq(a => a.Id, application.Id),
            update,
            options);

        await _audit.LogAsync(new AuditLog
        {
            EntityType = nameof(ApplicationRole),
            EntityId   = roleId,
            Action     = "Deleted",
            UserId     = "system"
        });
    }

    public async Task SetRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds)
    {
        // Fetch the application that owns this role.
        var application = await _applications
            .Find(a => a.Roles.Any(r => r.Id == roleId))
            .FirstOrDefaultAsync();

        if (application == null) return;

        var role = application.Roles.FirstOrDefault(r => r.Id == roleId);
        if (role == null) return;

        var permIdSet = permissionIds.ToHashSet();

        // Resolve the requested permissions from the application's standalone permissions array.
        var resolvedPermissions = application.Permissions
            .Where(p => permIdSet.Contains(p.Id))
            .ToList();

        // Replace the role's embedded permissions list with the resolved set.
        // Stored as RolePermission join objects referencing the permission.
        role.RolePermissions = resolvedPermissions
            .Select(p => new RolePermission
            {
                RoleId       = roleId,
                PermissionId = p.Id,
                Permission   = p
            })
            .ToList();

        // Persist the entire application document (role's RolePermissions have changed).
        application.UpdatedOn = DateTimeOffset.UtcNow;
        await _applications.ReplaceOneAsync(a => a.Id == application.Id, application);
        await _audit.LogAsync(new AuditLog
        {
            EntityType = nameof(ApplicationRole),
            EntityId   = roleId,
            Action     = "PermissionsUpdated",
            UserId     = "system"
        });
    }

    // ── Private helpers ────────────────────────────────────────────────────

    /// <summary>
    /// Resolves each <see cref="RolePermission.Permission"/> navigation on a role's
    /// <see cref="ApplicationRole.RolePermissions"/> collection using the application's
    /// standalone <paramref name="allPermissions"/> (avoids a second DB round-trip).
    /// </summary>
    private static void HydrateRolePermissions(
        ApplicationRole role,
        IEnumerable<ApplicationPermission> allPermissions)
    {
        if (role.RolePermissions is null or { Count: 0 }) return;

        var permMap = allPermissions.ToDictionary(p => p.Id);

        foreach (var rp in role.RolePermissions)
        {
            if (rp.Permission == null && permMap.TryGetValue(rp.PermissionId, out var perm))
                rp.Permission = perm;
        }
    }
}
