using CO.CDP.ApplicationRegistry.Persistence.Entities;
using CO.CDP.ApplicationRegistry.Persistence.MongoDB;
using MongoDB.Driver;

namespace CO.CDP.ApplicationRegistry.Persistence.Repositories.MongoDB;

/// <summary>
/// MongoDB concrete implementation of <see cref="IAccessControlRepository"/>.
/// Documents are stored flat in <c>app_registry_access_control</c>.
/// Revocation is a soft-delete: <c>RevokedAt</c> is set to the current UTC time.
/// </summary>
public class MongoAccessControlRepository : IAccessControlRepository
{
    private readonly IMongoCollection<AccessControlEntry> _entries;

    public MongoAccessControlRepository(MongoAppRegistryDatabase db)
    {
        _entries = db.AccessControl;
    }

    public async Task<IEnumerable<AccessControlEntry>> GetAclEntriesAsync(Guid reportId)
    {
        return await _entries
            .Find(e => e.ReportId == reportId && e.RevokedAt == null)
            .SortByDescending(e => e.GrantedAt)
            .ToListAsync();
    }

    public async Task<AccessControlEntry> GrantAccessAsync(AccessControlEntry entry)
    {
        entry.GrantedAt = DateTimeOffset.UtcNow;
        await _entries.InsertOneAsync(entry);
        return entry;
    }

    public async Task RevokeAccessAsync(Guid entryId)
    {
        var update = Builders<AccessControlEntry>.Update
            .Set(e => e.RevokedAt, DateTimeOffset.UtcNow);

        await _entries.UpdateOneAsync(e => e.Id == entryId, update);
    }
}
