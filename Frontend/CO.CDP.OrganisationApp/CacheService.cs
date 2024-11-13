using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;

namespace CO.CDP.OrganisationApp;

public class CacheService(IDistributedCache cache) : ICacheService
{
    public async Task<T?> Get<T>(string key)
    {
        return FromByteArray<T>(await cache.GetAsync(key));
    }

    public async Task Set<T>(string key, T value, DistributedCacheEntryOptions options)
    {
        var cacheValue = ToByteArray(value);

        if (cacheValue != null)
        {
            await cache.SetAsync(key, cacheValue, options);
        }
    }

    public async Task Remove(string key)
    {
        await cache.RemoveAsync(key);
    }

    private static bool IsStringType<T>() => typeof(T) == typeof(string);

    private static byte[]? ToByteArray<T>(T? value)
    {
        if (value == null) return null;
        var stringValue = IsStringType<T>() ? (value as string) : JsonSerializer.Serialize(value);

        if (stringValue == null) return null;
        return Encoding.UTF8.GetBytes(stringValue);
    }

    private static T? FromByteArray<T>(byte[]? value)
    {
        if (value == null) return default;

        var stringValue = Encoding.UTF8.GetString(value);

        return IsStringType<T>() ? (T)(object)stringValue : JsonSerializer.Deserialize<T>(stringValue);
    }
}