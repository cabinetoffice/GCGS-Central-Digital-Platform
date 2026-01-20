using System.Text.Json;
using CO.CDP.ApplicationRegistry.Core.Interfaces;
using CO.CDP.ApplicationRegistry.Core.Models.Claims;
using Microsoft.Extensions.Caching.Distributed;

namespace CO.CDP.ApplicationRegistry.Infrastructure.Services;

/// <summary>
/// Caching decorator for claims service with 15-minute TTL.
/// </summary>
public class CachedClaimsService : IClaimsCacheService
{
    private readonly IClaimsService _claimsService;
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    public CachedClaimsService(IClaimsService claimsService, IDistributedCache cache)
    {
        _claimsService = claimsService;
        _cache = cache;
    }

    public async Task<UserClaims> GetUserClaimsAsync(string userPrincipalId, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(userPrincipalId);

        // Try to get from cache
        var cachedValue = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedValue))
        {
            var claims = JsonSerializer.Deserialize<UserClaims>(cachedValue);
            if (claims != null)
            {
                return claims;
            }
        }

        // Get from service
        var userClaims = await _claimsService.GetUserClaimsAsync(userPrincipalId, cancellationToken);

        // Cache the result
        var serialized = JsonSerializer.Serialize(userClaims);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration
        };

        await _cache.SetStringAsync(cacheKey, serialized, options, cancellationToken);

        return userClaims;
    }

    public async Task InvalidateCacheAsync(string userPrincipalId, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(userPrincipalId);
        await _cache.RemoveAsync(cacheKey, cancellationToken);
    }

    private static string GetCacheKey(string userPrincipalId)
    {
        return $"user-claims:{userPrincipalId}";
    }
}
