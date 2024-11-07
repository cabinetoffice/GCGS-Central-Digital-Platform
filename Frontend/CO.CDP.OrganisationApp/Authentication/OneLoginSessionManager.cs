using Microsoft.Extensions.Caching.Distributed;

namespace CO.CDP.OrganisationApp.Authentication;

public class OneLoginSessionManager(
    IConfiguration config,
    IDistributedCache cache) : IOneLoginSessionManager
{
    private const string Value = "1";
    private double? _sessionTimeoutInMinutes;

    public void AddUserLoggedOut(string userUrn)
    {
        _sessionTimeoutInMinutes ??= config.GetValue<double>("SessionTimeoutInMinutes");

        cache.SetString(userUrn, Value, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_sessionTimeoutInMinutes.Value)
        });
    }

    public void RemoveUserLoggedOut(string userUrn)
    {
        cache.Remove(userUrn);
    }

    public bool IsLoggedOut(string userUrn)
    {
        return cache.GetString(userUrn) == Value;
    }
}