using CO.CDP.ApplicationRegistry.Persistence.Entities;

namespace CO.CDP.ApplicationRegistry.Persistence.Repositories;

public interface IFeatureFlagRepository
{
    Task<FeatureFlag?> GetByTileIdAsync(string tileId);
    Task<FeatureFlag> UpsertAsync(FeatureFlag flag);
    Task<IEnumerable<FeatureFlagScope>> GetScopesAsync(string tileId);
    Task SetScopesAsync(string tileId, IEnumerable<Guid> organisationTypeIds, Guid updatedBy);
    Task ClearScopesAsync(string tileId);
}
