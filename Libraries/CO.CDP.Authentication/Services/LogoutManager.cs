using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CO.CDP.Authentication.Services;

public class LogoutManager : ILogoutManager
{
    private const string Value = "1";
    public const string LogoutCallbackHttpClientName = "LogoutCallbackHttpClient";
    private readonly IConfiguration _config;
    private readonly ICacheService _cache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LogoutManager> _logger;

    public LogoutManager(
        IConfiguration config,
        ICacheService cache,
        IHttpClientFactory httpClientFactory,
        ILogger<LogoutManager> logger)
    {
        _config = config;
        _cache = cache;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task MarkAsLoggedOut(string userUrn, string logoutToken)
    {
        await AddToLoggedOutSessionsList(userUrn);
        await LogoutNotificationCallback(logoutToken);
    }

    public async Task RemoveAsLoggedOut(string userUrn)
    {
        await _cache.Remove(LoggedOutUserKey(userUrn));
    }

    public async Task<bool> HasLoggedOut(string userUrn)
    {
        return await _cache.Get<string?>(LoggedOutUserKey(userUrn)) == Value;
    }

    private async Task AddToLoggedOutSessionsList(string userUrn)
    {
        await _cache.Set(LoggedOutUserKey(userUrn), Value, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_config.GetValue<double>("SessionTimeoutInMinutes"))
        });
    }

    private async Task LogoutNotificationCallback(string logoutToken)
    {
        var callbackUrls = _config.GetValue<string>("OneLogin:ForwardLogoutNotificationUrls");
        var apiKey = _config.GetValue<string>("OneLogin:ForwardLogoutNotificationApiKey") ?? "";

        if (string.IsNullOrWhiteSpace(apiKey))
            _logger.LogInformation("Missing OneLogin:ForwardLogoutNotificationApiKey environment variable");

        if (callbackUrls != null)
        {
            var httpClient = _httpClientFactory.CreateClient(LogoutCallbackHttpClientName);

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
                                Content = new FormUrlEncodedContent([new("logout_token", logoutToken)])
                            });

                        _logger.LogInformation("Logout callback to {Site}, Status: {StatusCode}", callbackUrl, response.StatusCode);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation(ex, "Logout callback to {Site} failed", callbackUrl);
                    }
                });

            await Task.WhenAll(tasks);
        }
        else
        {
            _logger.LogInformation("Forward logout notification urls configuration not available.");
        }
    }

    private static string LoggedOutUserKey(string userUrn) => $"LoggedOutUser_{userUrn}";
}
