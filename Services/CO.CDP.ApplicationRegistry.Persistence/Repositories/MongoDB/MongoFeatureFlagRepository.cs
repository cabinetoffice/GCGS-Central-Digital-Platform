using CO.CDP.ApplicationRegistry.Persistence.Entities;
using CO.CDP.ApplicationRegistry.Persistence.MongoDB;
using MongoDB.Driver;

namespace CO.CDP.ApplicationRegistry.Persistence.Repositories.MongoDB;

/// <summary>
/// MongoDB concrete implementation of <see cref="IFeatureFlagRepository"/>.
///
/// Document model:
///   <c>app_registry_feature_flags</c> — each document embeds its <c>scopes[]</c>
///   (FeatureFlagScope). Upsert is driven by the unique <c>tileId</c> field.
/// </summary>
public class MongoFeatureFlagRepository : IFeatureFlagRepository
{
    private readonly IMongoCollection<FeatureFlag> _flags;

    public MongoFeatureFlagRepository(MongoAppRegistryDatabase db)
    {
        _flags = db.FeatureFlags;
    }

    public async Task<FeatureFlag?> GetByTileIdAsync(string tileId)
    {
        return await _flags
            .Find(f => f.TileId == tileId)
            .FirstOrDefaultAsync();
    }

    public async Task<FeatureFlag> UpsertAsync(FeatureFlag flag)
    {
        flag.UpdatedAt = DateTimeOffset.UtcNow;

        var options = new ReplaceOptions { IsUpsert = true };
        await _flags.ReplaceOneAsync(f => f.TileId == flag.TileId, flag, options);
        return flag;
    }

    public async Task<IEnumerable<FeatureFlagScope>> GetScopesAsync(string tileId)
    {
        var flag = await _flags
            .Find(f => f.TileId == tileId)
            .FirstOrDefaultAsync();

        return flag?.Scopes ?? [];
    }

    public async Task SetScopesAsync(string tileId, IEnumerable<Guid> organisationTypeIds, Guid updatedBy)
    {
        var flag = await _flags
            .Find(f => f.TileId == tileId)
            .FirstOrDefaultAsync();

        if (flag == null) return;

        flag.Scopes = organisationTypeIds
            .Select(orgTypeId => new FeatureFlagScope
            {
                FeatureFlagId      = flag.Id,
                OrganisationTypeId = orgTypeId
            })
            .ToList();

        flag.UpdatedBy = updatedBy;
        flag.UpdatedAt = DateTimeOffset.UtcNow;

        await _flags.ReplaceOneAsync(f => f.TileId == tileId, flag);
    }

    public async Task ClearScopesAsync(string tileId)
    {
        var update = Builders<FeatureFlag>.Update
            .Set(f => f.Scopes, new List<FeatureFlagScope>())
            .Set(f => f.UpdatedAt, DateTimeOffset.UtcNow);

        await _flags.UpdateOneAsync(f => f.TileId == tileId, update);
    }
}
