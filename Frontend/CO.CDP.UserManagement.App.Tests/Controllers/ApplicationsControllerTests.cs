using CO.CDP.UserManagement.App.Controllers;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Responses;
using CO.CDP.UserManagement.WebApiClient;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.UserManagement.App.Tests.Controllers;

public class ApplicationsControllerTests
{
    private readonly Mock<IApplicationService> _applicationService;
    private readonly Mock<UserManagementClient> _apiClient;
    private readonly ApplicationsController _controller;

    public ApplicationsControllerTests()
    {
        _applicationService = new Mock<IApplicationService>();
        _apiClient = new Mock<UserManagementClient>("http://localhost", new HttpClient());
        _controller = new ApplicationsController(_applicationService.Object, _apiClient.Object);
    }

    [Fact]
    public async Task Index_WhenModelStateInvalid_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError("error", "invalid");

        var result = await _controller.Index("org", null, null, null, null, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Index_WithCdpOrganisationId_RedirectsToSlug()
    {
        var orgId = Guid.NewGuid();
        _apiClient.Setup(client => client.ByCdpGuidAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganisationResponse
            {
                Id = 1,
                CdpOrganisationGuid = orgId,
                Name = "Org",
                Slug = "org",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            });

        var result = await _controller.Index(null, orgId, "cat", "enabled", "search", CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.RouteValues.Should().ContainKey("organisationSlug").WhoseValue.Should().Be("org");
    }

    [Fact]
    public async Task Index_WithCdpOrganisationIdNotFound_ReturnsNotFound()
    {
        var orgId = Guid.NewGuid();
        _apiClient.Setup(client => client.ByCdpGuidAsync(orgId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Not Found", 404, string.Empty, new Dictionary<string, IEnumerable<string>>(), null));

        var result = await _controller.Index(null, orgId, null, null, null, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Index_WhenOrganisationSlugMissing_ReturnsNotFound()
    {
        var result = await _controller.Index(null, null, null, null, null, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Index_WhenViewModelNull_ReturnsNotFound()
    {
        _applicationService.Setup(service => service.GetApplicationsViewModelAsync("org", null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationsViewModel?)null);

        var result = await _controller.Index("org", null, null, null, null, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Index_WhenViewModelAvailable_ReturnsView()
    {
        var viewModel = ApplicationsViewModel.Empty with { OrganisationName = "Org" };
        _applicationService.Setup(service => service.GetApplicationsViewModelAsync("org", null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.Index("org", null, null, null, null, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task Enable_WhenParamsMissing_ReturnsNotFound()
    {
        var result = await _controller.Enable("", "", CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Enable_WhenViewModelNull_ReturnsNotFound()
    {
        _applicationService.Setup(service => service.GetEnableApplicationViewModelAsync("org", "app", It.IsAny<CancellationToken>()))
            .ReturnsAsync((EnableApplicationViewModel?)null);

        var result = await _controller.Enable("org", "app", CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Enable_WhenViewModelAvailable_ReturnsView()
    {
        var viewModel = EnableApplicationViewModel.Empty with { OrganisationName = "Org" };
        _applicationService.Setup(service => service.GetEnableApplicationViewModelAsync("org", "app", It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.Enable("org", "app", CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task EnablePost_WhenNotConfirmed_ReturnsView()
    {
        var viewModel = EnableApplicationViewModel.Empty with { OrganisationName = "Org" };
        _applicationService.Setup(service => service.GetEnableApplicationViewModelAsync("org", "app", It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.Enable("org", "app", false, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task EnablePost_WhenServiceFails_ReturnsNotFound()
    {
        _applicationService.Setup(service => service.EnableApplicationAsync("org", "app", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.Enable("org", "app", true, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task EnablePost_WhenSuccess_Redirects()
    {
        _applicationService.Setup(service => service.EnableApplicationAsync("org", "app", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.Enable("org", "app", true, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ApplicationsController.EnableSuccess));
    }

    [Fact]
    public async Task EnableSuccess_WhenViewModelNull_ReturnsNotFound()
    {
        _applicationService.Setup(service => service.GetEnableSuccessViewModelAsync("org", "app", It.IsAny<CancellationToken>()))
            .ReturnsAsync((EnableApplicationSuccessViewModel?)null);

        var result = await _controller.EnableSuccess("org", "app", CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task EnableSuccess_WhenViewModelAvailable_ReturnsView()
    {
        var viewModel = EnableApplicationSuccessViewModel.Empty with { OrganisationName = "Org" };
        _applicationService.Setup(service => service.GetEnableSuccessViewModelAsync("org", "app", It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.EnableSuccess("org", "app", CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task Details_WhenViewModelNull_ReturnsNotFound()
    {
        _applicationService.Setup(service => service.GetApplicationDetailsViewModelAsync("org", "app", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationDetailsViewModel?)null);

        var result = await _controller.Details("org", "app", CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Details_WhenViewModelAvailable_ReturnsView()
    {
        var viewModel = ApplicationDetailsViewModel.Empty with { OrganisationName = "Org" };
        _applicationService.Setup(service => service.GetApplicationDetailsViewModelAsync("org", "app", It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.Details("org", "app", CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task Disable_WhenViewModelNull_ReturnsNotFound()
    {
        _applicationService.Setup(service => service.GetDisableApplicationViewModelAsync("org", "app", It.IsAny<CancellationToken>()))
            .ReturnsAsync((DisableApplicationViewModel?)null);

        var result = await _controller.Disable("org", "app", CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Disable_WhenViewModelAvailable_ReturnsView()
    {
        var viewModel = DisableApplicationViewModel.Empty with { OrganisationName = "Org" };
        _applicationService.Setup(service => service.GetDisableApplicationViewModelAsync("org", "app", It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.Disable("org", "app", CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task DisablePost_WhenNotConfirmed_ReturnsView()
    {
        var viewModel = DisableApplicationViewModel.Empty with { OrganisationName = "Org" };
        _applicationService.Setup(service => service.GetDisableApplicationViewModelAsync("org", "app", It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.Disable("org", "app", false, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task DisablePost_WhenServiceFails_ReturnsNotFound()
    {
        _applicationService.Setup(service => service.DisableApplicationAsync("org", "app", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.Disable("org", "app", true, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DisablePost_WhenSuccess_RedirectsToDetails()
    {
        _applicationService.Setup(service => service.DisableApplicationAsync("org", "app", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.Disable("org", "app", true, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ApplicationsController.Details));
    }
}
