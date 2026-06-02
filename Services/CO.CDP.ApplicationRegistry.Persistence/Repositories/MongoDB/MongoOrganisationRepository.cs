using CO.CDP.ApplicationRegistry.Persistence.Entities;
using CO.CDP.ApplicationRegistry.Persistence.MongoDB;
using MongoDB.Driver;

namespace CO.CDP.ApplicationRegistry.Persistence.Repositories.MongoDB;

/// <summary>
/// MongoDB concrete implementation of <see cref="IOrganisationRepository"/>.
///
/// Document model:
///   <c>app_registry_organisations</c> — embeds <c>members[]</c> (UserOrganisationMembership)
///   and an <c>enabledApplications[]</c> sub-document array (replaces the EF Core
///   OrganisationApplication join table). Application details are resolved by batch-fetching
///   the <c>app_registry_applications</c> collection.
/// </summary>
public class MongoOrganisationRepository : IOrganisationRepository
{
    private readonly IMongoCollection<Organisation>  _organisations;
    private readonly IMongoCollection<Application>   _applications;
    private readonly IAuditRepository                _audit;

    public MongoOrganisationRepository(MongoAppRegistryDatabase db, IAuditRepository audit)
    {
        _organisations = db.Organisations;
        _applications  = db.Applications;
        _audit         = audit;
    }

    // ── Read operations ────────────────────────────────────────────────────

    public async Task<Organisation?> GetByIdAsync(Guid id)
    {
        return await _organisations
            .Find(o => o.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<Organisation?> GetBySlugAsync(string slug)
    {
        return await _organisations
            .Find(o => o.Slug == slug)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Organisation>> GetAllAsync(string? name = null, string? type = null)
    {
        var filter = Builders<Organisation>.Filter.Eq(o => o.IsActive, true);

        if (!string.IsNullOrWhiteSpace(name))
            filter &= Builders<Organisation>.Filter.Regex(o => o.Name, name);

        if (!string.IsNullOrWhiteSpace(type))
            filter &= Builders<Organisation>.Filter.Eq(o => o.Type, type);

        return await _organisations
            .Find(filter)
            .SortBy(o => o.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserOrganisationMembership>> GetMembersAsync(Guid organisationId)
    {
        var org = await _organisations
            .Find(o => o.Id == organisationId)
            .FirstOrDefaultAsync();

        return org?.Members
                   .Where(m => m.IsActive)
                   .OrderBy(m => m.JoinedAt)
                   .ToList()
               ?? [];
    }

    public async Task<UserOrganisationMembership?> GetMemberAsync(Guid organisationId, string userPrincipalId)
    {
        var org = await _organisations
            .Find(o => o.Id == organisationId)
            .FirstOrDefaultAsync();

        return org?.Members
            .FirstOrDefault(m => m.UserPrincipalId == userPrincipalId && m.IsActive);
    }

    public async Task<IEnumerable<OrganisationApplication>> GetOrganisationApplicationsAsync(Guid organisationId)
    {
        var org = await _organisations
            .Find(o => o.Id == organisationId)
            .FirstOrDefaultAsync();

        if (org == null || org.Applications.Count == 0) return [];

        // Resolve application details by batch-fetching from the applications collection.
        var appIds = org.Applications.Select(oa => oa.ApplicationId).ToList();
        var apps = await _applications
            .Find(a => appIds.Contains(a.Id) && a.IsActive)
            .ToListAsync();

        var appMap = apps.ToDictionary(a => a.Id);

        return org.Applications
            .Where(oa => appMap.ContainsKey(oa.ApplicationId))
            .Select(oa =>
            {
                oa.Application  = appMap[oa.ApplicationId];
                oa.Organisation = org;
                return oa;
            })
            .ToList();
    }

    // ── Write operations ───────────────────────────────────────────────────

    public async Task<Organisation> CreateAsync(Organisation organisation)
    {
        organisation.CreatedOn = DateTimeOffset.UtcNow;
        organisation.UpdatedOn = DateTimeOffset.UtcNow;
        await _organisations.InsertOneAsync(organisation);
        await _audit.LogAsync(new AuditLog
        {
            EntityType = nameof(Organisation),
            EntityId   = organisation.Id,
            Action     = "Created",
            UserId     = "system"
        });
        return organisation;
    }

    public async Task UpdateAsync(Organisation organisation)
    {
        organisation.UpdatedOn = DateTimeOffset.UtcNow;
        await _organisations.ReplaceOneAsync(o => o.Id == organisation.Id, organisation);
        await _audit.LogAsync(new AuditLog
        {
            EntityType = nameof(Organisation),
            EntityId   = organisation.Id,
            Action     = "Updated",
            UserId     = "system"
        });
    }

    public async Task AddMemberAsync(UserOrganisationMembership membership)
    {
        var update = Builders<Organisation>.Update
            .Push(o => o.Members, membership);

        await _organisations.UpdateOneAsync(o => o.Id == membership.OrganisationId, update);
        await _audit.LogAsync(new AuditLog
        {
            EntityType = nameof(UserOrganisationMembership),
            EntityId   = membership.Id,
            Action     = "Created",
            UserId     = "system"
        });
    }

    public async Task UpdateMemberAsync(UserOrganisationMembership membership)
    {
        // Replace the matching embedded member document.
        var update = Builders<Organisation>.Update
            .Set("members.$[m].organisationRole", membership.OrganisationRole)
            .Set("members.$[m].isActive",         membership.IsActive);

        var options = new UpdateOptions
        {
            ArrayFilters = [new BsonDocumentArrayFilterDefinition<global::MongoDB.Bson.BsonDocument>(
                global::MongoDB.Bson.BsonDocument.Parse($"{{\"m.id\": \"{membership.Id}\"}}"))]
        };

        await _organisations.UpdateOneAsync(
            Builders<Organisation>.Filter.Eq(o => o.Id, membership.OrganisationId),
            update,
            options);

        await _audit.LogAsync(new AuditLog
        {
            EntityType = nameof(UserOrganisationMembership),
            EntityId   = membership.Id,
            Action     = "Updated",
            UserId     = "system"
        });
    }

    public async Task EnableApplicationAsync(OrganisationApplication organisationApplication)
    {
        var update = Builders<Organisation>.Update
            .Push(o => o.Applications, organisationApplication);

        await _organisations.UpdateOneAsync(
            o => o.Id == organisationApplication.OrganisationId,
            update);

        await _audit.LogAsync(new AuditLog
        {
            EntityType = nameof(OrganisationApplication),
            EntityId   = organisationApplication.ApplicationId,
            Action     = "ApplicationEnabled",
            UserId     = "system"
        });
    }

    public async Task DisableApplicationAsync(Guid organisationId, Guid applicationId)
    {
        var update = Builders<Organisation>.Update
            .PullFilter(o => o.Applications, oa => oa.ApplicationId == applicationId);

        await _organisations.UpdateOneAsync(o => o.Id == organisationId, update);
        await _audit.LogAsync(new AuditLog
        {
            EntityType = nameof(OrganisationApplication),
            EntityId   = applicationId,
            Action     = "ApplicationDisabled",
            UserId     = "system"
        });
    }
}
