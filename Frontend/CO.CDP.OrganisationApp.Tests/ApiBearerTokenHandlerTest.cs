using CO.CDP.OrganisationApp.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using static CO.CDP.OrganisationApp.ApiBearerTokenHandler;

namespace CO.CDP.OrganisationApp.Tests;

public class ApiBearerTokenHandlerTest
{
    private readonly Mock<ISession> SessionMock = new();

    [Fact]
    public async Task SendAsync_ShouldAttachBearerToken_WhenUserDetailsArePresent()
    {
        var handler = CreateHandlerWithUserDetails(
            authToken: GetAuthToken("existing-access-token", "existing-refresh-token"));

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        var response = await handler.SendAsync(request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        request.Headers.Authorization?.Parameter.Should().Be("existing-access-token");

        SessionMock.Verify(x => x.Set(Session.UserDetailsKey, It.IsAny<UserDetails>()), Times.Never);
    }

    [Fact]
    public async Task SendAsync_ShouldRefreshToken_WhenAccessTokenIsExpired()
    {
        var handler = CreateHandlerWithUserDetails(
            authToken: GetAuthToken("expired-access-token", "valid-refresh-token", -5),
            newTokenResponse: CreateTokenResponse());

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        var response = await handler.SendAsync(request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        request.Headers.Authorization?.Parameter.Should().Be("new-access-token");

        SessionMock.Verify(x => x.Set(Session.UserDetailsKey, It.IsAny<UserDetails>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_ShouldGetNewToken_WhenNoTokensArePresent()
    {
        var handler = CreateHandlerWithUserDetails(
            authToken: null,
            newTokenResponse: CreateTokenResponse());

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        var response = await handler.SendAsync(request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        request.Headers.Authorization?.Parameter.Should().Be("new-access-token");

        SessionMock.Verify(x => x.Set(Session.UserDetailsKey, It.IsAny<UserDetails>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_ShouldThrowException_WhenAccessTokenCannotBeObtained()
    {
        var handler = CreateHandlerWithUserDetails(authToken: null);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        var act = async () => await handler.SendAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task SendAsync_ShouldHandleTokenRenewalFailure_Gracefully()
    {
        var handler = CreateHandlerWithUserDetails(
            authToken: GetAuthToken("expired-access-token", "valid-refresh-token", -5));

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        var act = async () => await handler.SendAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }

    private static TokenResponse CreateTokenResponse()
    {
        return new TokenResponse
        {
            TokenType = "Bearer",
            AccessToken = "new-access-token",
            ExpiresIn = 3600,
            RefreshToken = "new-refresh-token",
            RefreshTokenExpiresIn = 86400
        };
    }

    private static AuthTokens GetAuthToken(string accessToken, string refreshToken, int accessTokenExpiryInMinutes = 5)
    {
        return new AuthTokens
        {
            AccessToken = accessToken,
            AccessTokenExpiry = DateTime.Now.AddMinutes(accessTokenExpiryInMinutes),
            RefreshToken = refreshToken,
            RefreshTokenExpiry = DateTime.Now.AddDays(1)
        };
    }

    private TestableApiBearerTokenHandler CreateHandlerWithUserDetails(AuthTokens? authToken = null, TokenResponse? newTokenResponse = null)
    {
        var httpContextAccessor = GetHttpContextAccessor();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();

        SessionMock.Setup(x => x.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test:urn", AuthTokens = authToken });

        httpClientFactoryMock.Setup(x => x.CreateClient("OrganisationAuthorityHttpClient"))
           .Returns(
               new HttpClient(new TestHandler(new HttpResponseMessage
               {
                   StatusCode = newTokenResponse == null ? HttpStatusCode.BadRequest : HttpStatusCode.OK,
                   Content = new StringContent(newTokenResponse == null ? "" : JsonSerializer.Serialize(newTokenResponse))
               }))
               { BaseAddress = new Uri("http://test.com") }
           );

        return new TestableApiBearerTokenHandler(httpContextAccessor, httpClientFactoryMock.Object, SessionMock.Object)
        {
            InnerHandler = new TestHandler(new HttpResponseMessage(HttpStatusCode.OK))
        };
    }

    private static IHttpContextAccessor GetHttpContextAccessor()
    {
        var authResult = AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(), ""));
        authResult.Properties!.StoreTokens([new AuthenticationToken { Name = "access_token", Value = "accessTokenValue" }]);

        var authenticationServiceMock = new Mock<IAuthenticationService>();
        authenticationServiceMock
            .Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>(), null))
            .ReturnsAsync(authResult);

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(_ => _.GetService(typeof(IAuthenticationService))).Returns(authenticationServiceMock.Object);

        return new HttpContextAccessor { HttpContext = new DefaultHttpContext { RequestServices = serviceProvider.Object } };
    }

    private class TestableApiBearerTokenHandler(
        IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory, ISession session)
        : ApiBearerTokenHandler(httpContextAccessor, httpClientFactory, session)
    {
        public new Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken);
        }
    }

    private class TestHandler(HttpResponseMessage response) : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(response);
        }
    }
}