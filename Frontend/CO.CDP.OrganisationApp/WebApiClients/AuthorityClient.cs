using CO.CDP.OrganisationApp.Authentication;
using CO.CDP.OrganisationApp.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json.Serialization;
using static IdentityModel.OidcConstants;

namespace CO.CDP.OrganisationApp.WebApiClients;

public class AuthorityClient(
    IConfiguration config,
    ICacheService cache,
    ITokenService tokenService,
    IHttpClientFactory httpClientFactory) : IAuthorityClient
{
    public const string OrganisationAuthorityHttpClientName = "OrganisationAuthorityHttpClient";

    public async Task<AuthTokens?> GetAuthTokens(string? userUrn)
        => await GetAuthTokens(userUrn, true);

    public async Task RevokeRefreshToken(string? userUrn)
    {
        var tokens = await GetAuthTokens(userUrn, false);

        if (tokens == null || tokens.RefreshTokenExpiry <= DateTime.Now) return;

        var request = new HttpRequestMessage(HttpMethod.Post, "/revocation")
        {
            Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>(TokenIntrospectionRequest.Token, tokens.RefreshToken),
                new KeyValuePair<string, string>(TokenIntrospectionRequest.TokenTypeHint, TokenTypes.RefreshToken)
            ])
        };

        var response = await GetClient().SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Unable to revoke refresh token from Organisation Authority, " +
                    $"{response.StatusCode} {await response.Content.ReadAsStringAsync()}");
        }
    }

    private async Task<AuthTokens?> GetAuthTokens(string? userUrn, bool fetchIfNotAvailable = true)
    {
        if (userUrn == null) return null;

        var cacheKey = AuthTokensCacheKey(userUrn);
        var tokens = await cache.Get<AuthTokens>(cacheKey);

        if (fetchIfNotAvailable)
        {
            (bool newToken, tokens) = await GetAuthTokens(tokens);
            if (newToken)
            {
                await cache.Set(cacheKey, tokens, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(config.GetValue<double>("SessionTimeoutInMinutes"))
                });
            }
        }
        else
        {
            await cache.Remove(cacheKey);
        }

        return tokens;
    }

    private async Task<(bool newToken, AuthTokens tokens)> GetAuthTokens(AuthTokens? tokens)
    {
        var newToken = false;

        if (tokens == null || tokens.RefreshTokenExpiry < DateTime.Now)
        {
            var oneLogintoken = await GetOneloginAccessToken();
            tokens = await GetAccessTokenAsync(GrantTypes.ClientCredentials, TokenRequest.ClientSecret, oneLogintoken);
            newToken = true;
        }

        if (DateTime.Now > tokens.AccessTokenExpiry && DateTime.Now < tokens.RefreshTokenExpiry)
        {
            tokens = await GetAccessTokenAsync(GrantTypes.RefreshToken, TokenRequest.RefreshToken, tokens.RefreshToken);
            newToken = true;
        }

        return (newToken, tokens);
    }

    private async Task<string> GetOneloginAccessToken()
    {
        var oneLogintoken = await tokenService.GetTokenAsync("access_token");

        if (!string.IsNullOrWhiteSpace(oneLogintoken))
        {
            return oneLogintoken;
        }

        throw new Exception("User logged out");
    }

    private async Task<AuthTokens> GetAccessTokenAsync(string grantType, string tokenRequestType, string credential)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/token")
        {
            Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>(TokenRequest.GrantType, grantType),
                new KeyValuePair<string, string>(tokenRequestType, credential)
            ])
        };

        var response = await GetClient().SendAsync(request);

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

    private HttpClient GetClient()
    {
        return httpClientFactory.CreateClient(OrganisationAuthorityHttpClientName);
    }

    private static string AuthTokensCacheKey(string userUrn) => $"UserAuthTokens_{userUrn}";

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