using CO.CDP.ApplicationRegistry.App;
using CO.CDP.ApplicationRegistry.App.Authentication;
using CO.CDP.ApplicationRegistry.App.WebApiClients;
using CO.CDP.ApplicationRegistry.Core.Tokens;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using System.Net;

namespace CO.CDP.ApplicationRegistry.UnitTests;

public class AuthorityClientTests
{
    private readonly Mock<IAppSession> _sessionMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly FakeTimeProvider _timeProvider;
    private readonly AuthorityClient _client;
    private readonly MockHttpMessageHandler _httpHandler;

    public AuthorityClientTests()
    {
        _sessionMock = new Mock<IAppSession>();
        _tokenServiceMock = new Mock<ITokenService>();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _timeProvider = new FakeTimeProvider(new DateTimeOffset(2024, 1, 15, 12, 0, 0, TimeSpan.Zero));

        _httpHandler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(_httpHandler)
        {
            BaseAddress = new Uri("https://authority.example.com")
        };

        httpClientFactoryMock
            .Setup(f => f.CreateClient(AuthorityClient.OrganisationAuthorityHttpClientName))
            .Returns(httpClient);

        _client = new AuthorityClient(
            _sessionMock.Object,
            _tokenServiceMock.Object,
            httpClientFactoryMock.Object,
            _timeProvider);
    }

    [Fact]
    public async Task GetAuthTokens_NullUserUrn_ReturnsNoUserError()
    {
        var result = await _client.GetAuthTokens(null);

        result.Match(
            error => error.Should().BeOfType<AuthorityClientError.NoUser>(),
            _ => Assert.Fail("Expected failure but got success")
        );
    }

    [Fact]
    public async Task GetAuthTokens_ValidExistingTokens_ReturnsExistingTokens()
    {
        var existingTokens = new AuthTokens
        {
            AccessToken = "existing-access",
            AccessTokenExpiry = _timeProvider.GetUtcNow().AddMinutes(30),
            RefreshToken = "existing-refresh",
            RefreshTokenExpiry = _timeProvider.GetUtcNow().AddHours(1)
        };

        _sessionMock.Setup(s => s.Get<AuthTokens>(Session.UserAuthTokens))
            .Returns(existingTokens);
        _tokenServiceMock.Setup(t => t.GetTokenAsync("expires_at"))
            .ReturnsAsync(_timeProvider.GetUtcNow().AddMinutes(60).ToString("o"));
        _tokenServiceMock.Setup(t => t.GetTokenAsync("access_token"))
            .ReturnsAsync("one-login-token");

        var result = await _client.GetAuthTokens("user-urn");

        result.Match(
            _ => Assert.Fail("Expected success but got failure"),
            tokens => tokens.Should().Be(existingTokens)
        );
    }

    [Fact]
    public async Task GetAuthTokens_NoExistingTokens_FetchesNewTokens()
    {
        _sessionMock.Setup(s => s.Get<AuthTokens>(Session.UserAuthTokens))
            .Returns((AuthTokens?)null);
        _tokenServiceMock.Setup(t => t.GetTokenAsync("expires_at"))
            .ReturnsAsync(_timeProvider.GetUtcNow().AddMinutes(60).ToString("o"));
        _tokenServiceMock.Setup(t => t.GetTokenAsync("access_token"))
            .ReturnsAsync("one-login-token");

        var tokenResponseJson = """
            {
                "access_token": "new-access-token",
                "token_type": "Bearer",
                "expires_in": 3600,
                "refresh_token": "new-refresh-token",
                "refresh_expires_in": 86400
            }
            """;
        _httpHandler.SetupResponse(HttpStatusCode.OK, tokenResponseJson);

        var result = await _client.GetAuthTokens("user-urn");

        result.Match(
            _ => Assert.Fail("Expected success but got failure"),
            tokens =>
            {
                tokens.AccessToken.Should().Be("new-access-token");
                tokens.RefreshToken.Should().Be("new-refresh-token");
            }
        );
        _sessionMock.Verify(s => s.Set(Session.UserAuthTokens, It.IsAny<AuthTokens>()), Times.Once);
    }

    [Fact]
    public async Task GetAuthTokens_AccessExpired_RefreshValid_RefreshesTokens()
    {
        var existingTokens = new AuthTokens
        {
            AccessToken = "expired-access",
            AccessTokenExpiry = _timeProvider.GetUtcNow().AddMinutes(-5),
            RefreshToken = "valid-refresh",
            RefreshTokenExpiry = _timeProvider.GetUtcNow().AddMinutes(30)
        };

        _sessionMock.Setup(s => s.Get<AuthTokens>(Session.UserAuthTokens))
            .Returns(existingTokens);
        _tokenServiceMock.Setup(t => t.GetTokenAsync("expires_at"))
            .ReturnsAsync(_timeProvider.GetUtcNow().AddMinutes(60).ToString("o"));
        _tokenServiceMock.Setup(t => t.GetTokenAsync("access_token"))
            .ReturnsAsync("one-login-token");

        var tokenResponseJson = """
            {
                "access_token": "refreshed-access-token",
                "token_type": "Bearer",
                "expires_in": 3600,
                "refresh_token": "refreshed-refresh-token",
                "refresh_expires_in": 86400
            }
            """;
        _httpHandler.SetupResponse(HttpStatusCode.OK, tokenResponseJson);

        var result = await _client.GetAuthTokens("user-urn");

        result.Match(
            _ => Assert.Fail("Expected success but got failure"),
            tokens => tokens.AccessToken.Should().Be("refreshed-access-token")
        );
    }

    [Fact]
    public async Task GetAuthTokens_OneLoginExpired_ReturnsError()
    {
        _sessionMock.Setup(s => s.Get<AuthTokens>(Session.UserAuthTokens))
            .Returns((AuthTokens?)null);
        _tokenServiceMock.Setup(t => t.GetTokenAsync("expires_at"))
            .ReturnsAsync(_timeProvider.GetUtcNow().AddMinutes(-10).ToString("o"));
        _tokenServiceMock.Setup(t => t.GetTokenAsync("access_token"))
            .ReturnsAsync("one-login-token");

        var result = await _client.GetAuthTokens("user-urn");

        result.Match(
            error => error.Should().BeOfType<AuthorityClientError.OneLoginTokenExpired>(),
            _ => Assert.Fail("Expected failure but got success")
        );
    }

    [Fact]
    public async Task GetAuthTokens_UserLoggedOut_ReturnsError()
    {
        _sessionMock.Setup(s => s.Get<AuthTokens>(Session.UserAuthTokens))
            .Returns((AuthTokens?)null);
        _tokenServiceMock.Setup(t => t.GetTokenAsync("expires_at"))
            .ReturnsAsync(_timeProvider.GetUtcNow().AddMinutes(10).ToString("o"));
        _tokenServiceMock.Setup(t => t.GetTokenAsync("access_token"))
            .ReturnsAsync((string?)null);

        var result = await _client.GetAuthTokens("user-urn");

        result.Match(
            error => error.Should().BeOfType<AuthorityClientError.UserLoggedOut>(),
            _ => Assert.Fail("Expected failure but got success")
        );
    }

    [Fact]
    public async Task RevokeRefreshToken_NullUserUrn_DoesNothing()
    {
        await _client.RevokeRefreshToken(null);

        _sessionMock.Verify(s => s.Get<AuthTokens>(Session.UserAuthTokens), Times.Never);
    }

    [Fact]
    public async Task RevokeRefreshToken_ValidToken_CallsRevocationEndpoint()
    {
        var tokens = new AuthTokens
        {
            AccessToken = "access",
            AccessTokenExpiry = _timeProvider.GetUtcNow().AddMinutes(30),
            RefreshToken = "refresh-to-revoke",
            RefreshTokenExpiry = _timeProvider.GetUtcNow().AddMinutes(30)
        };

        _sessionMock.Setup(s => s.Get<AuthTokens>(Session.UserAuthTokens))
            .Returns(tokens);
        _httpHandler.SetupResponse(HttpStatusCode.OK);

        await _client.RevokeRefreshToken("user-urn");

        _sessionMock.Verify(s => s.Remove(Session.UserAuthTokens), Times.Once);
        _httpHandler.LastRequest.Should().NotBeNull();
        _httpHandler.LastRequest!.RequestUri!.PathAndQuery.Should().Be("/revocation");
    }

    [Fact]
    public async Task RevokeRefreshToken_ExpiredToken_SkipsRevocation()
    {
        var tokens = new AuthTokens
        {
            AccessToken = "access",
            AccessTokenExpiry = _timeProvider.GetUtcNow().AddMinutes(-30),
            RefreshToken = "expired-refresh",
            RefreshTokenExpiry = _timeProvider.GetUtcNow().AddMinutes(-10)
        };

        _sessionMock.Setup(s => s.Get<AuthTokens>(Session.UserAuthTokens))
            .Returns(tokens);

        await _client.RevokeRefreshToken("user-urn");

        _sessionMock.Verify(s => s.Remove(Session.UserAuthTokens), Times.Once);
        _httpHandler.LastRequest.Should().BeNull();
    }

    [Fact]
    public async Task RevokeRefreshToken_NullTokens_SkipsRevocation()
    {
        _sessionMock.Setup(s => s.Get<AuthTokens>(Session.UserAuthTokens))
            .Returns((AuthTokens?)null);

        await _client.RevokeRefreshToken("user-urn");

        _sessionMock.Verify(s => s.Remove(Session.UserAuthTokens), Times.Once);
        _httpHandler.LastRequest.Should().BeNull();
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private HttpStatusCode _statusCode = HttpStatusCode.OK;
        private string _responseContent = string.Empty;

        public HttpRequestMessage? LastRequest { get; private set; }

        public void SetupResponse(HttpStatusCode statusCode, string? content = null)
        {
            _statusCode = statusCode;
            _responseContent = content ?? string.Empty;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseContent, System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
