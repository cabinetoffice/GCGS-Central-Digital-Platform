namespace CO.CDP.Authentication.Services;

public interface ICacheService
{
    Task Set<T>(string key, T value, Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions options);
    Task<T?> Get<T>(string key);
    Task Remove(string key);
}
