using CO.CDP.UI.Foundation.Services;
using CO.CDP.UserManagement.App.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace CO.CDP.UserManagement.App.Tests.Controllers;

public class LogoutControllerTests
{
    private const string SirsiHomePage = "https://sirsi.example.com/";

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

    private static LogoutController CreateController(bool authenticated)
    {
        var identity = authenticated
            ? new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user")], "test")
            : new ClaimsIdentity();

        var sirsiUrlService = new Mock<ISirsiUrlService>();
        sirsiUrlService.Setup(x => x.BuildUrl("/", null, null, null)).Returns(SirsiHomePage);

        return new LogoutController(sirsiUrlService.Object)
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
