using CO.CDP.ApplicationRegistry.Persistence.Entities;
using CO.CDP.ApplicationRegistry.Persistence.MongoDB;
using MongoDB.Driver;

namespace CO.CDP.ApplicationRegistry.Persistence.Repositories.MongoDB;

public class MongoAuditRepository : IAuditRepository
{
    private readonly IMongoCollection<AuditLog> _collection;

    public MongoAuditRepository(MongoAppRegistryDatabase db)
    {
        _collection = db.AuditLogs;
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
        string? entityType = null,
        string? action = null,
        string? userId = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        int limit = 100,
        int offset = 0)
    {
        var filter = Builders<AuditLog>.Filter.Empty;

        if (!string.IsNullOrWhiteSpace(entityType))
            filter &= Builders<AuditLog>.Filter.Eq(a => a.EntityType, entityType);

        if (!string.IsNullOrWhiteSpace(action))
            filter &= Builders<AuditLog>.Filter.Eq(a => a.Action, action);

        if (!string.IsNullOrWhiteSpace(userId))
            filter &= Builders<AuditLog>.Filter.Eq(a => a.UserId, userId);

        if (from.HasValue)
            filter &= Builders<AuditLog>.Filter.Gte(a => a.Timestamp, from.Value);

        if (to.HasValue)
            filter &= Builders<AuditLog>.Filter.Lte(a => a.Timestamp, to.Value);

        return await _collection
            .Find(filter)
            .SortByDescending(a => a.Timestamp)
            .Skip(offset)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task LogAsync(AuditLog log, CancellationToken ct = default)
    {
        log.Timestamp = DateTimeOffset.UtcNow;
        await _collection.InsertOneAsync(log, cancellationToken: ct);
    }
}
