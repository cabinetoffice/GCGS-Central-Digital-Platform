using CO.CDP.Functional;
using CO.CDP.RegisterOfCommercialTools.WebApi.Helpers;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Services.Caching;

public interface IRedisCacheService
{
    Task<Option<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default);

    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration,
        CancellationToken cancellationToken = default);
}

public class RedisCacheService(
    IDistributedCache cache,
    ILogger<RedisCacheService> logger) : IRedisCacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<Option<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default) =>
        await TryGetFromCache<T>(key, cancellationToken);

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration,
        CancellationToken cancellationToken = default) =>
        await TrySetInCache(key, value, expiration, cancellationToken);

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);

        return await cached.Match(
            some: Task.FromResult,
            none: async () =>
            {
                var value = await factory();
                await SetAsync(key, value, expiration, cancellationToken);
                return value;
            });
    }

    private async Task<Option<T>> TryGetFromCache<T>(string key, CancellationToken cancellationToken)
    {
        try
        {
            var bytes = await cache.GetAsync(key, cancellationToken);
            return bytes == null
                ? LogCacheMiss<T>(key)
                : Deserialize<T>(bytes).Match(
                    some: value => LogCacheHit(key, value),
                    none: () => LogDeserializationError<T>(key));
        }
        catch (Exception ex)
        {
            return LogCacheError<T>(key, ex);
        }
    }

    private async Task TrySetInCache<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken)
    {
        try
        {
            var bytes = Serialize(value);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            await cache.SetAsync(key, bytes, options, cancellationToken);
            logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", LogSanitizer.Sanitize(key), expiration);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to set cache for key: {Key}", LogSanitizer.Sanitize(key));
        }
    }

    private static Option<T> Deserialize<T>(byte[] bytes)
    {
        try
        {
            var json = Encoding.UTF8.GetString(bytes);
            var value = JsonSerializer.Deserialize<T>(json, JsonOptions);
            return value == null ? Option<T>.None : Option<T>.Some(value);
        }
        catch
        {
            return Option<T>.None;
        }
    }

    private static byte[] Serialize<T>(T value)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        return Encoding.UTF8.GetBytes(json);
    }

    private Option<T> LogCacheMiss<T>(string key)
    {
        logger.LogDebug("Cache miss for key: {Key}", LogSanitizer.Sanitize(key));
        return Option<T>.None;
    }

    private Option<T> LogCacheHit<T>(string key, T value)
    {
        logger.LogDebug("Cache hit for key: {Key}", LogSanitizer.Sanitize(key));
        return Option<T>.Some(value);
    }

    private Option<T> LogDeserializationError<T>(string key)
    {
        logger.LogWarning("Failed to deserialize cached value for key: {Key}", LogSanitizer.Sanitize(key));
        return Option<T>.None;
    }

    private Option<T> LogCacheError<T>(string key, Exception ex)
    {
        logger.LogWarning(ex, "Error retrieving from cache for key: {Key}", LogSanitizer.Sanitize(key));
        return Option<T>.None;
    }
}