using Microsoft.Extensions.Caching.Distributed;

namespace CO.CDP.OrganisationApp.Authentication;

public class LogoutManager(
    IConfiguration config,
    ICacheService cache,
    IHttpClientFactory httpClientFactory,
    ILogger<LogoutManager> logger) : ILogoutManager
{
    private const string Value = "1";
    public const string LogoutCallbackHttpClientName = "LogoutCallbackHttpClient";

    public async Task MarkAsLoggedOut(string userUrn, string logout_token)
    {
        await AddToLoggedOutSessionsList(userUrn);
        await LogoutNotificationCallback(logout_token);
    }

    public async Task RemoveAsLoggedOut(string userUrn)
    {
        await cache.Remove(LoggedOutUserKey(userUrn));
    }

    public async Task<bool> HasLoggedOut(string userUrn)
    {
        return await cache.Get<string?>(LoggedOutUserKey(userUrn)) == Value;
    }

    private async Task AddToLoggedOutSessionsList(string userUrn)
    {
        await cache.Set(LoggedOutUserKey(userUrn), Value, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(config.GetValue<double>("SessionTimeoutInMinutes"))
        });
    }

    private async Task LogoutNotificationCallback(string logout_token)
    {
        var callbackUrls = config.GetValue<string>("OneLogin:ForwardLogoutNotificationUrls");
        var apiKey = config.GetValue<string>("OneLogin:ForwardLogoutNotificationApiKey") ?? "";

        if (string.IsNullOrWhiteSpace(apiKey))
            logger.LogInformation("Missing OneLogin:ForwardLogoutNotificationApiKey environment variable");

        if (callbackUrls != null)
        {
            var httpClient = httpClientFactory.CreateClient(LogoutCallbackHttpClientName);

            var tasks = callbackUrls
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(async callbackUrl =>
                {
                    try
                    {
                        var response = await httpClient.SendAsync(
                            new HttpRequestMessage
                            {
                                Method = HttpMethod.Post,
                                RequestUri = new Uri(callbackUrl),
                                Headers = { { "sirsi-logout-api-key", apiKey } },
                                Content = new FormUrlEncodedContent([new("logout_token", logout_token)])
                            });

                        logger.LogInformation("Logout callback to {Site}, Status: {StatusCode}", callbackUrl, response.StatusCode);
                    }
                    catch (Exception ex)
                    {
                        logger.LogInformation(ex, "Logout callback to {Site} failed", callbackUrl);
                    }
                });

            await Task.WhenAll(tasks);
        }
        else
        {
            logger.LogInformation("Forward logout notification urls configuration not available.");
        }
    }

    private static string LoggedOutUserKey(string userUrn) => $"LoggedOutUser_{userUrn}";
}