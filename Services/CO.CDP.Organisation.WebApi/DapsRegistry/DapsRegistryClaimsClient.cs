using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace CO.CDP.Organisation.WebApi.DapsRegistry;

/// <summary>
/// Connection to the DAPS Application Registry, the source of truth for application roles and
/// permissions. CDP is registered there as a consuming application (client credentials).
/// </summary>
public class DapsRegistryOptions
{
    public const string Section = "DapsApplicationRegistry";

    /// <summary>Registry API base URL, e.g. https://platform.cdp.codatplat.com/application-registry-api. Empty = off.</summary>
    public string BaseUrl { get; set; } = "";

    public string ClientId { get; set; } = "";

    public string ClientSecret { get; set; } = "";

    /// <summary>Token issuance waits on this call, so keep it short; on timeout cdp_claims carries no applications.</summary>
    public int TimeoutSeconds { get; set; } = 5;

    public bool Enabled =>
        !string.IsNullOrWhiteSpace(BaseUrl) && !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);
}

/// <summary>One registry organisation's application roles for a user (DAPS <c>OrganisationClaimsDto</c>).</summary>
public record DapsOrganisationClaims(
    [property: JsonPropertyName("organisationId")] string OrganisationId,
    [property: JsonPropertyName("organisationName")] string OrganisationName,
    [property: JsonPropertyName("cdpOrganisationId")] string? CdpOrganisationId,
    [property: JsonPropertyName("applications")] IReadOnlyList<DapsApplicationClaims> Applications);

/// <summary>DAPS <c>ApplicationClaimsDto</c>: the discriminator is the application's client id in cdp_claims.</summary>
public record DapsApplicationClaims(
    [property: JsonPropertyName("applicationId")] string ApplicationId,
    [property: JsonPropertyName("discriminator")] string Discriminator,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("roles")] IReadOnlyList<string> Roles,
    [property: JsonPropertyName("permissions")] IReadOnlyList<string> Permissions);

public interface IDapsRegistryClaimsClient
{
    /// <summary>
    /// The user's application roles per registry organisation, or null when the registry is not
    /// configured or did not answer (the caller then issues cdp_claims without applications).
    /// </summary>
    Task<IReadOnlyList<DapsOrganisationClaims>?> GetClaimsAsync(string userUrn, CancellationToken cancellationToken = default);
}

public class DapsRegistryClaimsClient(
    IHttpClientFactory httpClientFactory,
    DapsRegistryOptions options,
    ILogger<DapsRegistryClaimsClient> logger) : IDapsRegistryClaimsClient
{
    public const string HttpClientName = "DapsApplicationRegistry";

    private static readonly TimeSpan RenewBefore = TimeSpan.FromMinutes(2);

    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _token;
    private DateTimeOffset _tokenExpiresAt = DateTimeOffset.MinValue;

    public async Task<IReadOnlyList<DapsOrganisationClaims>?> GetClaimsAsync(string userUrn, CancellationToken cancellationToken = default)
    {
        if (!options.Enabled)
        {
            return null;
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(options.TimeoutSeconds));
        try
        {
            var client = httpClientFactory.CreateClient(HttpClientName);
            var token = await GetTokenAsync(client, timeout.Token);

            using var request = new HttpRequestMessage(HttpMethod.Get, Url($"api/claims/{Uri.EscapeDataString(userUrn)}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var response = await client.SendAsync(request, timeout.Token);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("DAPS Application Registry claims returned {Status}; cdp_claims will carry no applications", (int)response.StatusCode);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _token = null;
                }

                return null;
            }

            var body = await response.Content.ReadFromJsonAsync<DapsUserClaims>(cancellationToken: timeout.Token);
            return body?.Organisations ?? [];
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or System.Text.Json.JsonException)
        {
            logger.LogWarning(ex, "DAPS Application Registry claims call failed; cdp_claims will carry no applications");
            return null;
        }
    }

    private async Task<string> GetTokenAsync(HttpClient client, CancellationToken cancellationToken)
    {
        if (_token is not null && _tokenExpiresAt - RenewBefore > DateTimeOffset.UtcNow)
        {
            return _token;
        }

        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            if (_token is not null && _tokenExpiresAt - RenewBefore > DateTimeOffset.UtcNow)
            {
                return _token;
            }

            using var response = await client.PostAsJsonAsync(Url("api/clients/token"),
                new { clientId = options.ClientId, clientSecret = options.ClientSecret }, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"DAPS Application Registry token request returned {(int)response.StatusCode}");
            }

            var token = await response.Content.ReadFromJsonAsync<DapsToken>(cancellationToken: cancellationToken)
                        ?? throw new HttpRequestException("DAPS Application Registry token response was empty");
            _token = token.AccessToken;
            _tokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn);
            return _token;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private string Url(string path) => options.BaseUrl.TrimEnd('/') + "/" + path;

    private record DapsUserClaims(
        [property: JsonPropertyName("organisations")] IReadOnlyList<DapsOrganisationClaims> Organisations);

    private record DapsToken(
        [property: JsonPropertyName("accessToken")] string AccessToken,
        [property: JsonPropertyName("expiresIn")] int ExpiresIn);
}
