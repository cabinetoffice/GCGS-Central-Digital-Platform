using CO.CDP.Authentication.Services;
using CO.CDP.UserManagement.App.Authentication;
using CO.CDP.UI.Foundation.Services;
using CO.CDP.UserManagement.App.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace CO.CDP.UserManagement.App.Tests.Controllers;

public class LogoutControllerTests
{
    private const string SirsiHomePage = "https://sirsi.example.com/";
    private readonly Mock<ISirsiUrlService> _sirsiUrlService = new();
    private readonly Mock<IOneLoginAuthority> _oneLoginAuthority = new();
    private readonly Mock<ILogoutManager> _logoutManager = new();
    private readonly Mock<ILogger<LogoutController>> _logger = new();

    public LogoutControllerTests()
    {
        _sirsiUrlService.Setup(x => x.BuildUrl("/", null, null, null)).Returns(SirsiHomePage);
    }

    [Fact]
    public void Index_WhenUserAuthenticated_ReturnsSignOutResult()
    {
        var controller = CreateController(authenticated: true);

        var result = controller.Index();

        result.Should().BeOfType<SignOutResult>();
        var signOutResult = (SignOutResult)result;
        signOutResult.AuthenticationSchemes.Should().Contain([
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme
        ]);
        signOutResult.Properties?.RedirectUri.Should().Be(SirsiHomePage);
    }

    [Fact]
    public void Index_WhenUserNotAuthenticated_RedirectsToSirsiHomePage()
    {
        var controller = CreateController(authenticated: false);

        var result = controller.Index();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be(SirsiHomePage);
    }

    [Fact]
    public void Index_UsesLogoutAndAuthLogoutRoutes()
    {
        var method = typeof(LogoutController).GetMethod(nameof(LogoutController.Index));

        method.Should().NotBeNull();
        method!
            .GetCustomAttributes(typeof(HttpGetAttribute), false)
            .Cast<HttpGetAttribute>()
            .Select(attribute => attribute.Template)
            .Should()
            .BeEquivalentTo("/logout", "/Auth/Logout");
    }

    [Fact]
    public async Task BackChannelSignOut_WhenTokenMissing_ReturnsBadRequest()
    {
        var controller = CreateController();

        var result = await controller.BackChannelSignOut(null);

        result.Should().BeOfType<BadRequestObjectResult>();
        _logoutManager.Verify(m => m.MarkAsLoggedOut(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task BackChannelSignOut_WhenTokenInvalid_ReturnsBadRequest()
    {
        const string token = "invalid";
        _oneLoginAuthority.Setup(x => x.ValidateLogoutToken(token)).ReturnsAsync((string?)null);
        var controller = CreateController();

        var result = await controller.BackChannelSignOut(token);

        result.Should().BeOfType<BadRequestObjectResult>();
        _logoutManager.Verify(m => m.MarkAsLoggedOut(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task BackChannelSignOut_WhenTokenValid_MarksSessionAndReturnsOk()
    {
        const string token = "valid-token";
        const string urn = "urn:gov:one-login:123";
        _oneLoginAuthority.Setup(x => x.ValidateLogoutToken(token)).ReturnsAsync(urn);
        _logoutManager.Setup(x => x.MarkAsLoggedOut(urn, token)).Returns(Task.CompletedTask);
        var controller = CreateController();

        var result = await controller.BackChannelSignOut(token);

        result.Should().BeOfType<OkResult>();
        _logoutManager.Verify(x => x.MarkAsLoggedOut(urn, token), Times.Once);
    }

    [Fact]
    public void BackChannelSignOut_UsesBackChannelSignOutRoute()
    {
        var method = typeof(LogoutController).GetMethod(nameof(LogoutController.BackChannelSignOut));

        method.Should().NotBeNull();
        method!
            .GetCustomAttributes(typeof(HttpMethodAttribute), false)
            .Cast<HttpMethodAttribute>()
            .Select(attribute => attribute.Template)
            .Should()
            .Contain("/one-login/back-channel-sign-out");
    }

    [Fact]
    public void BackChannelSignOut_AllowsAnonymousAtControllerLevel_AndIgnoresAntiforgeryOnAction()
    {
        var controllerType = typeof(LogoutController);
        var method = controllerType.GetMethod(nameof(LogoutController.BackChannelSignOut));

        controllerType.GetCustomAttributes(typeof(AllowAnonymousAttribute), false).Should().ContainSingle();

        method.Should().NotBeNull();
        method!.GetCustomAttributes(typeof(IgnoreAntiforgeryTokenAttribute), false).Should().ContainSingle();
    }

    [Fact]
    public void BackChannelSignOut_ConsumesFormUrlEncodedPayload()
    {
        var method = typeof(LogoutController).GetMethod(nameof(LogoutController.BackChannelSignOut));

        method.Should().NotBeNull();
        method!
            .GetCustomAttributes(typeof(ConsumesAttribute), false)
            .Cast<ConsumesAttribute>()
            .SelectMany(attribute => attribute.ContentTypes)
            .Should()
            .Contain("application/x-www-form-urlencoded");
    }

    private LogoutController CreateController(bool authenticated = false)
    {
        var identity = authenticated
            ? new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user")], "test")
            : new ClaimsIdentity();

        return new LogoutController(
            _sirsiUrlService.Object,
            _oneLoginAuthority.Object,
            _logoutManager.Object,
            _logger.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            }
        };
    }
}
