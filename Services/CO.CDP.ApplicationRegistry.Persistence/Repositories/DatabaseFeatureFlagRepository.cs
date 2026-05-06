using CO.CDP.ApplicationRegistry.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.ApplicationRegistry.Persistence.Repositories;

public class DatabaseFeatureFlagRepository : IFeatureFlagRepository
{
    private readonly ApplicationRegistryContext _context;

    public DatabaseFeatureFlagRepository(ApplicationRegistryContext context)
    {
        _context = context;
    }

    public async Task<FeatureFlag?> GetByTileIdAsync(string tileId)
    {
        return await _context.FeatureFlags
            .Include(f => f.Scopes)
            .FirstOrDefaultAsync(f => f.TileId == tileId);
    }

    public async Task<FeatureFlag> UpsertAsync(FeatureFlag flag)
    {
        var existing = await _context.FeatureFlags
            .FirstOrDefaultAsync(f => f.TileId == flag.TileId);

        if (existing != null)
        {
            existing.Enabled = flag.Enabled;
            existing.Reason = flag.Reason;
            existing.UpdatedBy = flag.UpdatedBy;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            _context.FeatureFlags.Add(flag);
        }

        await _context.SaveChangesAsync();
        return existing ?? flag;
    }

    public async Task<IEnumerable<FeatureFlagScope>> GetScopesAsync(string tileId)
    {
        var flag = await _context.FeatureFlags
            .Include(f => f.Scopes)
            .FirstOrDefaultAsync(f => f.TileId == tileId);

        return flag?.Scopes ?? new List<FeatureFlagScope>();
    }

    public async Task SetScopesAsync(string tileId, IEnumerable<Guid> organisationTypeIds, Guid updatedBy)
    {
        var flag = await _context.FeatureFlags
            .Include(f => f.Scopes)
            .FirstOrDefaultAsync(f => f.TileId == tileId);

        if (flag == null) return;

        _context.FeatureFlagScopes.RemoveRange(flag.Scopes);

        foreach (var orgTypeId in organisationTypeIds)
        {
            flag.Scopes.Add(new FeatureFlagScope
            {
                FeatureFlagId = flag.Id,
                OrganisationTypeId = orgTypeId
            });
        }

        flag.UpdatedBy = updatedBy;
        flag.UpdatedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task ClearScopesAsync(string tileId)
    {
        var flag = await _context.FeatureFlags
            .Include(f => f.Scopes)
            .FirstOrDefaultAsync(f => f.TileId == tileId);

        if (flag == null) return;

        _context.FeatureFlagScopes.RemoveRange(flag.Scopes);
        await _context.SaveChangesAsync();
    }
}
