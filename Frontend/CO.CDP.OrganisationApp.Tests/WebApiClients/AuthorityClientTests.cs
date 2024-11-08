using CO.CDP.OrganisationApp.Authentication;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using FluentAssertions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace CO.CDP.OrganisationApp.Tests.WebApiClients;

public class AuthorityClientTests
{
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly AuthorityClient _authorityClient;

    public AuthorityClientTests()
    {
        _authorityClient = new AuthorityClient(_tokenServiceMock.Object, _httpClientFactoryMock.Object);
    }

    [Fact]
    public async Task GetAuthTokens_ShouldRequestNewToken_WhenTokensAreNull()
    {
        var authTokens = GivenToken("new-access-token", "new-refresh-token", 5, 10);

        _tokenServiceMock.Setup(t => t.GetTokenAsync("access_token"))
            .ReturnsAsync("onelogin-access-token");

        MockHttpClientFactory(authTokens);

        var (newToken, tokens) = await _authorityClient.GetAuthTokens(null);

        newToken.Should().BeTrue();
        tokens.Should().NotBeNull();
        tokens.AccessToken.Should().Be(authTokens.AccessToken);
        tokens.RefreshToken.Should().Be(authTokens.RefreshToken);
    }

    [Fact]
    public async Task GetAuthTokens_ShouldRequestNewToken_WhenAccessTokenAndRefreshTokenAreExpired()
    {
        var expiredTokens = GivenToken("expired-access-token", "expired-refresh-token", -10, -5);

        var newTokens = GivenToken("new-access-token", "valid-refresh-token", 5, 10);

        _tokenServiceMock.Setup(t => t.GetTokenAsync("access_token"))
            .ReturnsAsync("onelogin-access-token");

        MockHttpClientFactory(newTokens);

        var (newToken, tokens) = await _authorityClient.GetAuthTokens(expiredTokens);

        newToken.Should().BeTrue();
        tokens.AccessToken.Should().Be(newTokens.AccessToken);
    }

    [Fact]
    public async Task GetAuthTokens_ShouldRefreshAccessToken_WhenAccessTokenIsExpiredAndRefreshTokenIsValid()
    {
        var expiredTokens = GivenToken("expired-access-token", "valid-refresh-token", -5, 10);

        var newTokens = GivenToken("new-access-token", "valid-refresh-token", 5, 10);

        MockHttpClientFactory(newTokens);

        var (newToken, tokens) = await _authorityClient.GetAuthTokens(expiredTokens);

        newToken.Should().BeTrue();
        tokens.AccessToken.Should().Be(newTokens.AccessToken);
    }

    [Fact]
    public async Task GetOneloginAccessToken_ShouldThrowException_WhenTokenIsMissing()
    {
        Func<Task> act = async () => await _authorityClient.GetAuthTokens(null);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("User logged out");
    }

    private void MockHttpClientFactory(AuthTokens authTokens)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new AuthorityClient.TokenResponse
            {
                TokenType = "Bearer",
                AccessToken = authTokens.AccessToken,
                RefreshToken = authTokens.RefreshToken,
                ExpiresIn = (authTokens.AccessTokenExpiry - DateTime.Now).TotalSeconds,
                RefreshTokenExpiresIn = (authTokens.RefreshTokenExpiry - DateTime.Now).TotalSeconds
            }))
        };

        var httpClientHandlerMock = new Mock<HttpMessageHandler>();
        httpClientHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(httpClientHandlerMock.Object)
        {
            BaseAddress = new Uri("http://test.com")
        };

        _httpClientFactoryMock.Setup(f => f.CreateClient(AuthorityClient.OrganisationAuthorityHttpClientName))
            .Returns(httpClient);
    }

    private static AuthTokens GivenToken
        (string accessToken, string refreshToken, int accessTokenExpiryIn, int refreshTokenExpiryIn)
        => new()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = DateTime.Now.AddMinutes(accessTokenExpiryIn),
            RefreshTokenExpiry = DateTime.Now.AddMinutes(refreshTokenExpiryIn)
        };
}