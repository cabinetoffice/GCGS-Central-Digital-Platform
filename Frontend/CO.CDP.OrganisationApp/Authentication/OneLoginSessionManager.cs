using Microsoft.Extensions.Caching.Distributed;

namespace CO.CDP.OrganisationApp.Authentication;

public class OneLoginSessionManager(
    IConfiguration config,
    IDistributedCache cache) : IOneLoginSessionManager
{
    private const string Value = "1";
    private double? _sessionTimeoutInMinutes;

    public void AddToSignedOutSessionsList(string userUrn)
    {
        _sessionTimeoutInMinutes ??= config.GetValue<double>("SessionTimeoutInMinutes");

        cache.SetString(userUrn, Value, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_sessionTimeoutInMinutes.Value)
        });
    }

    public void RemoveFromSignedOutSessionsList(string userUrn)
    {
        cache.Remove(userUrn);
    }

    public bool HasSignedOut(string userUrn)
    {
        return cache.GetString(userUrn) == Value;
    }
}