using CO.CDP.GovUKNotify.Models;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CO.CDP.GovUKNotify;

public class GovUKNotifyApiClient : IGovUKNotifyApiClient
{
    public const string GovUKNotifyHttpClientName = "GovUKNotifyHttpClient";

    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient client;

    public GovUKNotifyApiClient(IHttpClientFactory httpClientFactory, IAuthentication authentication)
    {
        client = httpClientFactory.CreateClient(GovUKNotifyHttpClientName);
        client.BaseAddress = new Uri("https://api.notifications.service.gov.uk");
        client.DefaultRequestHeaders.Authorization = authentication.GetAuthenticationHeader();
    }

    public async Task<EmailNotificationResponse?> SendEmail(EmailNotificationRequest request)
    {
        var res = await client.PostAsJsonAsync("/v2/notifications/email", request, jsonSerializerOptions);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<EmailNotificationResponse>();
    }
}