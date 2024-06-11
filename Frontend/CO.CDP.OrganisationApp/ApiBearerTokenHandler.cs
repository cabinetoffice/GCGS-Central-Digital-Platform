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
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var userDetails = session.Get<UserDetails>(Session.UserDetailsKey);

        if (userDetails != null)
        {
            if (userDetails.AccessToken == null)
            {
                var context = httpContextAccessor.HttpContext;
                if (context != null)
                {
                    var oneLogintoken = await context.GetTokenAsync("access_token");
                    // var expiresAt = DateTimeOffset.Parse(await context.GetTokenAsync("expires_at")).ToLocalTime();

                    if (!string.IsNullOrWhiteSpace(oneLogintoken))
                    {
                        var token = await GetAccessTokenAsync(oneLogintoken);
                        userDetails.AccessToken = token;
                        session.Set(Session.UserDetailsKey, userDetails);
                    }
                }
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userDetails.AccessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<string> GetAccessTokenAsync(string oneLogintoken)
    {
        using var httpClient = httpClientFactory.CreateClient("OrganisationAuthorityHttpClient");
        var request = new HttpRequestMessage(HttpMethod.Post, "/token")
        {
            Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>(TokenRequest.GrantType, GrantTypes.ClientCredentials),
                new KeyValuePair<string, string>(TokenRequest.ClientSecret, oneLogintoken)
            ])
        };

        var response = await httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var payload = await response.Content.ReadFromJsonAsync<TokenResponse>();
            return payload!.AccessToken;
        }
        else
        {
            throw new Exception($"Unable to get access token from Organisation Authority, " +
                $"{response.StatusCode} {await response.Content.ReadAsStringAsync()}");
        }
    }

    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public required string TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public double? ExpiresIn { get; set; }
    }
}