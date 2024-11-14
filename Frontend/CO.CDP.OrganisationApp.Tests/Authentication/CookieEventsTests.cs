using CO.CDP.OrganisationApp.Authentication;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace CO.CDP.OrganisationApp.Tests.Authentication;

public class CookieEventsTests
{
    private readonly Mock<IAuthenticationService> _authenticationService = new();
    private readonly Mock<IOneLoginSessionManager> _oneLoginSessionManagerMock = new();
    private readonly Mock<IAuthorityClient> _authorityClientMock = new();
    private readonly CookieEvents _cookieEvents;

    public CookieEventsTests()
    {
        _cookieEvents = new CookieEvents(_oneLoginSessionManagerMock.Object, _authorityClientMock.Object);
    }

    [Fact]
    public async Task ValidatePrincipal_ShouldDoNothing_WhenNotAuthenticated()
    {
        var context = CreateContext(authenticated: false);

        await _cookieEvents.ValidatePrincipal(context);

        _oneLoginSessionManagerMock.Verify(m => m.HasSignedOut(It.IsAny<string>()), Times.Never);
        _oneLoginSessionManagerMock.Verify(m => m.RemoveFromSignedOutSessionsList(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ValidatePrincipal_ShouldDoNothing_WhenUrnIsMissing()
    {
        var context = CreateContext(authenticated: true, urn: null);

        await _cookieEvents.ValidatePrincipal(context);

        _oneLoginSessionManagerMock.Verify(m => m.HasSignedOut(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ValidatePrincipal_ShouldDoNothing_WhenUserIsNotLoggedOut()
    {
        var urn = "user-urn";
        var context = CreateContext(authenticated: true, urn: urn);
        _oneLoginSessionManagerMock.Setup(m => m.HasSignedOut(urn)).ReturnsAsync(false);

        await _cookieEvents.ValidatePrincipal(context);

        _oneLoginSessionManagerMock.Verify(m => m.HasSignedOut(urn), Times.Once);
        _authorityClientMock.Verify(m => m.RevokeRefreshToken(urn), Times.Never);
    }

    [Fact]
    public async Task ValidatePrincipal_ShouldRejectPrincipalAndSignOut_WhenUserIsLoggedOut()
    {
        var urn = "user-urn";
        var context = CreateContext(authenticated: true, urn: urn);
        _oneLoginSessionManagerMock.Setup(m => m.HasSignedOut(urn)).ReturnsAsync(true);

        await _cookieEvents.ValidatePrincipal(context);

        _oneLoginSessionManagerMock.Verify(m => m.HasSignedOut(urn), Times.Once);
        _oneLoginSessionManagerMock.Verify(m => m.RemoveFromSignedOutSessionsList(urn), Times.Once);
        _authorityClientMock.Verify(m => m.RevokeRefreshToken(urn), Times.Once);
        _authenticationService.Verify(m => m.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()), Times.Once);
    }

    private CookieValidatePrincipalContext CreateContext(bool authenticated, string? urn = "user-urn")
    {
        List<Claim> claims = [];
        if (urn != null) claims.Add(new Claim("sub", urn ?? string.Empty));

        var identity = authenticated ? new ClaimsIdentity(claims, "mockAuthType") : new ClaimsIdentity();

        var ticket = new AuthenticationTicket(
            new ClaimsPrincipal(identity),
            new AuthenticationProperties(), "Cookies");

        _authenticationService
            .Setup(x => x.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IAuthenticationService)))
            .Returns(_authenticationService.Object);

        return new CookieValidatePrincipalContext(
            new DefaultHttpContext { RequestServices = serviceProviderMock.Object },
            new AuthenticationScheme("Cookies", "Cookies", typeof(CookieAuthenticationHandler)),
            new CookieAuthenticationOptions(),
            ticket);
    }
}