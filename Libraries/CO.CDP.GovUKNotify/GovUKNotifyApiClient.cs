using CO.CDP.GovUKNotify.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CO.CDP.GovUKNotify;

public class GovUKNotifyApiClient : IGovUKNotifyApiClient
{
    public const string GovUKNotifyHttpClientName = "GovUKNotifyHttpClient";
    private readonly ILogger<GovUKNotifyApiClient> _logger;

    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient client;
    private readonly IAuthentication authentication;

    public GovUKNotifyApiClient(
        IHttpClientFactory httpClientFactory,
        IAuthentication auth,
        ILogger<GovUKNotifyApiClient> logger)
    {
        _logger = logger;
        client = httpClientFactory.CreateClient(GovUKNotifyHttpClientName);
        client.BaseAddress = new Uri("https://api.notifications.service.gov.uk");
        authentication = auth;
    }

    public async Task<EmailNotificationResponse?> SendEmail(EmailNotificationRequest request)
    {
        client.DefaultRequestHeaders.Authorization = authentication.GetAuthenticationHeader();

        var res = await client.PostAsJsonAsync("/v2/notifications/email", request, jsonSerializerOptions);
        try
        {
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<EmailNotificationResponse>();
        }
        catch (Exception ex)
        {
            var content = await res.Content.ReadAsStringAsync();
            _logger.LogError(ex, $"GovUKNotify error, response status: {res.StatusCode}, response body: {content}");
            throw;
        }
    }
}