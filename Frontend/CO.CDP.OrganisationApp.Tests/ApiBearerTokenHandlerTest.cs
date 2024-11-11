using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using FluentAssertions;
using Moq;
using System.Net;

namespace CO.CDP.OrganisationApp.Tests;

public class ApiBearerTokenHandlerTests
{
    private readonly Mock<ISession> sessionMock = new();
    private readonly Mock<IAuthorityClient> authorityClientMock = new();
    private readonly TestableApiBearerTokenHandler handler;

    public ApiBearerTokenHandlerTests()
    {
        handler = new TestableApiBearerTokenHandler(sessionMock.Object, authorityClientMock.Object)
        {
            InnerHandler = new TestHandler()
        };
    }

    [Fact]
    public async Task SendAsync_ShouldProceedWithoutAuthorization_WhenUserDetailsAreNull()
    {
        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns((UserDetails?)null);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        var response = await handler.SendAsync(request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        request.Headers.Authorization.Should().BeNull();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SendAsync_ShouldAddAuthorizationHeader_WhenTokenIsValid(bool isNewToken)
    {
        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test:urn" });
        authorityClientMock.Setup(a => a.GetAuthTokens(It.IsAny<AuthTokens>()))
            .ReturnsAsync((isNewToken, GivenAuthToken()));
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        var response = await handler.SendAsync(request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        request.Headers.Authorization.Should().NotBeNull();
        request.Headers.Authorization!.Scheme.Should().Be("Bearer");
        request.Headers.Authorization.Parameter.Should().Be("access_token");
        sessionMock.Verify(s => s.Set(Session.UserDetailsKey, It.IsAny<UserDetails>()), isNewToken ? Times.Once : Times.Never);
    }

    private class TestableApiBearerTokenHandler(
        ISession session, IAuthorityClient authorityClient)
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

    private static AuthTokens GivenAuthToken()
    {
        return new AuthTokens
        {
            AccessToken = "access_token",
            AccessTokenExpiry = DateTime.Now.AddMinutes(60),
            RefreshToken = "refresh_token",
            RefreshTokenExpiry = DateTime.Now.AddDays(1)
        };
    }
}