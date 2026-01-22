using CO.CDP.Authentication.Models;
using System.Net.Http.Json;

namespace CO.CDP.Authentication.Services;

internal sealed class TokenExchangeService : ITokenExchangeService
{
    private const string HttpClientName = "AuthorityApi";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TimeProvider _timeProvider;

    public TokenExchangeService(IHttpClientFactory httpClientFactory, TimeProvider timeProvider)
    {
        _httpClientFactory = httpClientFactory;
        _timeProvider = timeProvider;
    }

    public async Task<AuthorityTokenSet> ExchangeOneLoginTokenAsync(string oneLoginToken, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(HttpClientName);

        var requestBody = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_secret", oneLoginToken }
        };

        var response = await httpClient.PostAsync(
            "/token",
            new FormUrlEncodedContent(requestBody),
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize token response");

        return AuthorityTokenSet.Create(
            tokenResponse.AccessToken,
            tokenResponse.RefreshToken,
            tokenResponse.ExpiresIn,
            tokenResponse.RefreshExpiresIn,
            _timeProvider.GetUtcNow());
    }

    public async Task<AuthorityTokenSet> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(HttpClientName);

        var requestBody = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken }
        };

        var response = await httpClient.PostAsync(
            "/token",
            new FormUrlEncodedContent(requestBody),
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize token response");

        return AuthorityTokenSet.Create(
            tokenResponse.AccessToken,
            tokenResponse.RefreshToken,
            tokenResponse.ExpiresIn,
            tokenResponse.RefreshExpiresIn,
            _timeProvider.GetUtcNow());
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(HttpClientName);

        var requestBody = new Dictionary<string, string>
        {
            { "token", refreshToken }
        };

        var response = await httpClient.PostAsync(
            "/revocation",
            new FormUrlEncodedContent(requestBody),
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}

