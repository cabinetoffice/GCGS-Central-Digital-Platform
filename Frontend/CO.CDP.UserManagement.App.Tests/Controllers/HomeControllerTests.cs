using CO.CDP.UserManagement.App.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.UserManagement.App.Tests.Controllers;

public class HomeControllerTests
{
    private readonly HomeController _controller = new();

    [Fact]
    public void Index_WithOrganisationId_RedirectsToUsers()
    {
        var organisationId = Guid.NewGuid();

        var result = _controller.Index(organisationId);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersListController.Index));
        redirect.ControllerName.Should().Be("UsersList");
        redirect.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(organisationId);
    }

    [Fact]
    public void Index_WhenOrganisationIdMissing_ReturnsNotFound()
    {
        var result = _controller.Index(null);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void Privacy_ReturnsView()
    {
        var result = _controller.Privacy();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Error_ReturnsViewWithModel()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = _controller.Error();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeNull();
    }

    [Fact]
    public void Error_HasErrorRoute()
    {
        var method = typeof(HomeController).GetMethod(nameof(HomeController.Error));

        method.Should().NotBeNull();
        method!
            .GetCustomAttributes(typeof(RouteAttribute), inherit: false)
            .Cast<RouteAttribute>()
            .Select(attribute => attribute.Template)
            .Should()
            .Contain("/error");
    }

    [Fact]
    public void PageNotFound_ReturnsView()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = _controller.PageNotFound();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void PageNotFound_HasExpectedRoute()
    {
        var method = typeof(HomeController).GetMethod(nameof(HomeController.PageNotFound));

        method.Should().NotBeNull();
        method!
            .GetCustomAttributes(typeof(RouteAttribute), inherit: false)
            .Cast<RouteAttribute>()
            .Select(attribute => attribute.Template)
            .Should()
            .Contain("/page-not-found");
    }
}
