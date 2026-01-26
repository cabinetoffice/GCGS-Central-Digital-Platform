using CO.CDP.Authentication.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace CO.CDP.Authentication.Tests;

public class CookieEventsServiceTests
{
    private readonly Mock<IAuthenticationService> _authenticationService = new();
    private readonly Mock<ILogoutManager> _logoutManagerMock = new();
    private readonly CookieEventsService _cookieEventsService;

    public CookieEventsServiceTests()
    {
        _cookieEventsService = new CookieEventsService(_logoutManagerMock.Object);
    }

    [Fact]
    public async Task ValidatePrincipal_ShouldDoNothing_WhenNotAuthenticated()
    {
        var context = CreateContext(authenticated: false);

        await _cookieEventsService.ValidatePrincipal(context);

        _logoutManagerMock.Verify(m => m.HasLoggedOut(It.IsAny<string>()), Times.Never);
        _logoutManagerMock.Verify(m => m.RemoveAsLoggedOut(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ValidatePrincipal_ShouldDoNothing_WhenUrnIsMissing()
    {
        var context = CreateContext(authenticated: true, urn: null);

        await _cookieEventsService.ValidatePrincipal(context);

        _logoutManagerMock.Verify(m => m.HasLoggedOut(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ValidatePrincipal_ShouldDoNothing_WhenUserIsNotLoggedOut()
    {
        var urn = "user-urn";
        var context = CreateContext(authenticated: true, urn: urn);
        _logoutManagerMock.Setup(m => m.HasLoggedOut(urn)).ReturnsAsync(false);

        await _cookieEventsService.ValidatePrincipal(context);

        _logoutManagerMock.Verify(m => m.HasLoggedOut(urn), Times.Once);
    }

    [Fact]
    public async Task ValidatePrincipal_ShouldRejectPrincipalAndSignOut_WhenUserIsLoggedOut()
    {
        var urn = "user-urn";
        var context = CreateContext(authenticated: true, urn: urn);
        _logoutManagerMock.Setup(m => m.HasLoggedOut(urn)).ReturnsAsync(true);

        await _cookieEventsService.ValidatePrincipal(context);

        _logoutManagerMock.Verify(m => m.HasLoggedOut(urn), Times.Once);
        _logoutManagerMock.Verify(m => m.RemoveAsLoggedOut(urn), Times.Once);
        _authenticationService.Verify(m => m.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()), Times.Once);
    }

    private CookieValidatePrincipalContext CreateContext(bool authenticated, string? urn = "user-urn")
    {
        List<Claim> claims = [];
        if (urn != null) claims.Add(new Claim("sub", urn));

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
