using CO.CDP.ApplicationRegistry.App;
using CO.CDP.ApplicationRegistry.App.Models;
using CO.CDP.ApplicationRegistry.App.WebApiClients;
using CO.CDP.ApplicationRegistry.Core.Tokens;
using CO.CDP.Functional;
using FluentAssertions;
using Moq;
using System.Net;

namespace CO.CDP.ApplicationRegistry.UnitTests;

public class ApiBearerTokenHandlerTests
{
    private readonly Mock<IAppSession> _sessionMock = new();
    private readonly Mock<IAuthorityClient> _authorityClientMock = new();
    private readonly TestableApiBearerTokenHandler _handler;

    public ApiBearerTokenHandlerTests()
    {
        _handler = new TestableApiBearerTokenHandler(_sessionMock.Object, _authorityClientMock.Object)
        {
            InnerHandler = new TestHandler()
        };
    }

    [Fact]
    public async Task SendAsync_ShouldProceedWithoutAuthorization_WhenUserDetailsAreNull()
    {
        _sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns((UserDetails?)null);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        var response = await _handler.SendAsync(request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        request.Headers.Authorization.Should().BeNull();
    }

    [Fact]
    public async Task SendAsync_ShouldAddAuthorizationHeader_WhenTokenIsValid()
    {
        var userDetails = new UserDetails { UserUrn = "urn:test:user:123" };
        var tokens = new AuthTokens
        {
            AccessToken = "test-access-token",
            AccessTokenExpiry = DateTimeOffset.Now.AddMinutes(60),
            RefreshToken = "test-refresh-token",
            RefreshTokenExpiry = DateTimeOffset.Now.AddDays(1)
        };

        _sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(userDetails);
        _authorityClientMock.Setup(a => a.GetAuthTokens(userDetails.UserUrn))
            .ReturnsAsync(Result<AuthorityClientError, AuthTokens>.Success(tokens));

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        var response = await _handler.SendAsync(request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        request.Headers.Authorization.Should().NotBeNull();
        request.Headers.Authorization!.Scheme.Should().Be("Bearer");
        request.Headers.Authorization.Parameter.Should().Be(tokens.AccessToken);
    }

    [Fact]
    public async Task SendAsync_ShouldNotAddAuthorizationHeader_WhenTokensFailure()
    {
        var userDetails = new UserDetails { UserUrn = "urn:test:user:123" };

        _sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(userDetails);
        _authorityClientMock.Setup(a => a.GetAuthTokens(userDetails.UserUrn))
            .ReturnsAsync(Result<AuthorityClientError, AuthTokens>.Failure(new AuthorityClientError.UserLoggedOut()));

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        var response = await _handler.SendAsync(request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        request.Headers.Authorization.Should().BeNull();
    }

    [Fact]
    public async Task SendAsync_ShouldCallAuthorityClientWithCorrectUserUrn()
    {
        var userDetails = new UserDetails { UserUrn = "urn:test:user:456" };

        _sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(userDetails);
        _authorityClientMock.Setup(a => a.GetAuthTokens(It.IsAny<string>()))
            .ReturnsAsync(Result<AuthorityClientError, AuthTokens>.Failure(new AuthorityClientError.NoUser()));

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        await _handler.SendAsync(request, CancellationToken.None);

        _authorityClientMock.Verify(a => a.GetAuthTokens("urn:test:user:456"), Times.Once);
    }

    private class TestableApiBearerTokenHandler(
        IAppSession session, IAuthorityClient authorityClient)
        : ApiBearerTokenHandler(session, authorityClient)
    {
        public new Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken);
        }
    }

    private class TestHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
