using Microsoft.Extensions.Caching.Distributed;

namespace CO.CDP.Authentication.Services;

public class DistributedCacheService(IDistributedCache cache) : ICacheService
{
    public async Task Set<T>(string key, T value, DistributedCacheEntryOptions options)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(value);
        await cache.SetStringAsync(key, json, options);
    }

    public async Task<T?> Get<T>(string key)
    {
        var json = await cache.GetStringAsync(key);
        return json == null ? default : System.Text.Json.JsonSerializer.Deserialize<T>(json);
    }

    public async Task Remove(string key)
    {
        await cache.RemoveAsync(key);
    }
}
