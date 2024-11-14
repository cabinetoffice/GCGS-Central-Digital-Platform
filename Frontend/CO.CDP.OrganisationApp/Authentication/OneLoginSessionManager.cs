using Microsoft.Extensions.Caching.Distributed;

namespace CO.CDP.OrganisationApp.Authentication;

public class OneLoginSessionManager(
    IConfiguration config,
    ICacheService cache) : IOneLoginSessionManager
{
    private const string Value = "1";

    public async Task AddToSignedOutSessionsList(string userUrn)
    {
        await cache.Set(SignedOutUserKey(userUrn), Value, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(config.GetValue<double>("SessionTimeoutInMinutes"))
        });
    }

    public async Task RemoveFromSignedOutSessionsList(string userUrn)
    {
        await cache.Remove(SignedOutUserKey(userUrn));
    }

    public async Task<bool> HasSignedOut(string userUrn)
    {
        return await cache.Get<string?>(SignedOutUserKey(userUrn)) == Value;
    }

    private static string SignedOutUserKey(string userUrn) => $"SignedOutUser_{userUrn}";
}