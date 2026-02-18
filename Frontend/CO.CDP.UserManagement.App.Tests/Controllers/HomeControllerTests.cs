using CO.CDP.UserManagement.App.Controllers;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Responses;
using CO.CDP.UserManagement.WebApiClient;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.UserManagement.App.Tests.Controllers;

public class HomeControllerTests
{
    private readonly Mock<IApplicationService> _applicationService;
    private readonly Mock<UserManagementClient> _apiClient;
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        _applicationService = new Mock<IApplicationService>();
        _apiClient = new Mock<UserManagementClient>("http://localhost", new HttpClient());
        _controller = new HomeController(_applicationService.Object, _apiClient.Object);
    }

    [Fact]
    public async Task Index_WithCdpOrganisationId_RedirectsToSlug()
    {
        var orgId = Guid.NewGuid();
        var org = new OrganisationResponse
        {
            Id = 1,
            CdpOrganisationGuid = orgId,
            Name = "Org",
            Slug = "org",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _apiClient.Setup(client => client.ByCdpGuidAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);

        var result = await _controller.Index(null, orgId, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(HomeController.Index));
        redirect.RouteValues.Should().ContainKey("organisationSlug").WhoseValue.Should().Be("org");
    }

    [Fact]
    public async Task Index_WithCdpOrganisationIdNotFound_ReturnsNotFound()
    {
        var orgId = Guid.NewGuid();
        _apiClient.Setup(client => client.ByCdpGuidAsync(orgId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Not Found", 404, string.Empty, new Dictionary<string, IEnumerable<string>>(), null));

        var result = await _controller.Index(null, orgId, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Index_WhenOrganisationSlugMissing_ReturnsNotFound()
    {
        var result = await _controller.Index(null, null, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Index_WhenSlugProvided_RedirectsToUsers()
    {
        var result = await _controller.Index("org", null, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.Index));
        redirect.ControllerName.Should().Be("Users");
    }

    [Fact]
    public async Task Index_WhenSlugProvided_DoesNotCallHomeService()
    {
        var result = await _controller.Index("org", null, CancellationToken.None);

        _applicationService.Verify(service => service.GetHomeViewModelAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        result.Should().BeOfType<RedirectToActionResult>();
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
        viewResult.Model.Should().BeOfType<ErrorViewModel>();
    }
}
