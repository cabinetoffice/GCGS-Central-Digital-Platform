using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using static IdentityModel.OidcConstants;

namespace CO.CDP.OrganisationApp;

public class ApiBearerTokenHandler(
    IHttpContextAccessor httpContextAccessor,
    IHttpClientFactory httpClientFactory,
    ISession session) : DelegatingHandler
{
    private readonly SemaphoreSlim semaphore = new(1, 1);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync();
        try
        {
            var userDetails = session.Get<UserDetails>(Session.UserDetailsKey);
            if (userDetails != null)
            {
                (bool newToken, AuthTokens tokens) = await GetAuthTokens(userDetails.AuthTokens);
                if (newToken)
                {
                    userDetails.AuthTokens = tokens;
                    session.Set(Session.UserDetailsKey, userDetails);
                }

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
            }
        }
        finally
        {
            semaphore.Release();
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<string> GetOneloginAccessToken()
    {
        var context = httpContextAccessor.HttpContext;
        if (context != null)
        {
            var oneLogintoken = await context.GetTokenAsync("access_token");
            // var expiresAt = DateTimeOffset.Parse(await context.GetTokenAsync("expires_at")).ToLocalTime();

            if (!string.IsNullOrWhiteSpace(oneLogintoken))
            {
                return oneLogintoken;
            }
        }

        throw new Exception("User logged out");
    }

    private async Task<(bool newToken, AuthTokens tokens)> GetAuthTokens(AuthTokens? authToken)
    {
        var newToken = false;

        if (authToken == null || authToken.RefreshTokenExpiry < DateTime.Now)
        {
            var oneLogintoken = await GetOneloginAccessToken();
            authToken = await GetAccessTokenAsync(GrantTypes.ClientCredentials, TokenRequest.ClientSecret, oneLogintoken);
            newToken = true;
        }

        if (DateTime.Now > authToken.AccessTokenExpiry && DateTime.Now < authToken.RefreshTokenExpiry)
        {
            authToken = await GetAccessTokenAsync(GrantTypes.RefreshToken, TokenRequest.RefreshToken, authToken.RefreshToken);
            newToken = true;
        }

        return (newToken, authToken);
    }

    private async Task<AuthTokens> GetAccessTokenAsync(string grantType, string tokenRequestType, string credential)
    {
        using var httpClient = httpClientFactory.CreateClient("OrganisationAuthorityHttpClient");
        var request = new HttpRequestMessage(HttpMethod.Post, "/token")
        {
            Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>(TokenRequest.GrantType, grantType),
                new KeyValuePair<string, string>(tokenRequestType, credential)
            ])
        };

        var response = await httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var payload = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (payload != null)
            {
                return new AuthTokens
                {
                    AccessToken = payload.AccessToken,
                    AccessTokenExpiry = DateTime.Now.AddSeconds(payload.ExpiresIn - 30),
                    RefreshToken = payload.RefreshToken,
                    RefreshTokenExpiry = DateTime.Now.AddSeconds(payload.RefreshTokenExpiresIn - 30)
                };
            }
        }

        throw new Exception($"Unable to get access token from Organisation Authority, " +
                $"{response.StatusCode} {await response.Content.ReadAsStringAsync()}");
    }

    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public required string TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public required double ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public required string RefreshToken { get; init; }

        [JsonPropertyName("refresh_expires_in")]
        public required double RefreshTokenExpiresIn { get; init; }
    }
}