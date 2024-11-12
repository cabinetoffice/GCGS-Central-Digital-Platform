using Microsoft.Extensions.Caching.Distributed;

namespace CO.CDP.OrganisationApp;

public interface ICacheService
{
    Task<T?> Get<T>(string key);

    Task Set<T>(string key, T value, DistributedCacheEntryOptions options);

    Task Remove(string key);
}