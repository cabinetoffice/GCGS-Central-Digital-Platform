using CO.CDP.Authentication.Services;
using CO.CDP.UserManagement.App.Authentication;
using CO.CDP.UserManagement.App.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.UserManagement.App.Tests.Controllers;

public class OneLoginControllerTests
{
    private readonly Mock<IOneLoginAuthority> _oneLoginAuthority = new();
    private readonly Mock<ILogoutManager> _logoutManager = new();
    private readonly Mock<ILogger<OneLoginController>> _logger = new();

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
    public void BackChannelSignOut_UsesSignoutOidcRoute()
    {
        var method = typeof(OneLoginController).GetMethod(nameof(OneLoginController.BackChannelSignOut));

        method.Should().NotBeNull();
        method!
            .GetCustomAttributes(typeof(HttpMethodAttribute), false)
            .Cast<HttpMethodAttribute>()
            .Select(attribute => attribute.Template)
            .Should()
            .Contain("/signout-oidc");
    }

    private OneLoginController CreateController()
        => new(_oneLoginAuthority.Object, _logoutManager.Object, _logger.Object);
}
