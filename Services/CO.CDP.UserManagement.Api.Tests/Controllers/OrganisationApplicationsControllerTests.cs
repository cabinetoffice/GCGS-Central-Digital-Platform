using CO.CDP.UserManagement.Api.Controllers;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using CoreOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Api.Tests.Controllers;

public class OrganisationApplicationsControllerTests
{
    private readonly Mock<IOrganisationApplicationService> _organisationApplicationService;
    private readonly OrganisationApplicationsController _controller;

    public OrganisationApplicationsControllerTests()
    {
        _organisationApplicationService = new Mock<IOrganisationApplicationService>();
        var logger = new Mock<ILogger<OrganisationApplicationsController>>();
        _controller = new OrganisationApplicationsController(_organisationApplicationService.Object, logger.Object);
    }

    [Fact]
    public async Task GetApplications_WhenOrganisationExists_ReturnsOk()
    {
        var orgApps = new List<OrganisationApplication>
        {
            new()
            {
                Id = 1,
                OrganisationId = 10,
                ApplicationId = 20,
                IsActive = true,
                CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                CreatedBy = "system",
                Organisation = new CoreOrganisation
                    { Id = 10, Name = "Org", Slug = "org", CreatedAt = DateTimeOffset.UtcNow },
                Application = new Application
                    { Id = 20, Name = "App", ClientId = "app", CreatedAt = DateTimeOffset.UtcNow }
            }
        };
        _organisationApplicationService
            .Setup(service => service.GetByOrganisationIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orgApps);

        var result = await _controller.GetApplications(10, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<IEnumerable<OrganisationApplicationResponse>>>()
            .Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<OrganisationApplicationResponse>>().Subject
            .ToList();
        response.Should().HaveCount(1);
        response[0].OrganisationId.Should().Be(10);
    }

    [Fact]
    public async Task GetApplications_WhenNotFound_ReturnsNotFound()
    {
        _organisationApplicationService
            .Setup(service => service.GetByOrganisationIdAsync(10, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Organisation", 10));

        var result = await _controller.GetApplications(10, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<IEnumerable<OrganisationApplicationResponse>>>()
            .Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetApplicationsByUser_ReturnsOk()
    {
        var orgApps = new List<OrganisationApplication>
        {
            new()
            {
                Id = 2,
                OrganisationId = 10,
                ApplicationId = 30,
                IsActive = true,
                CreatedAt = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero),
                CreatedBy = "system"
            }
        };
        _organisationApplicationService.Setup(service =>
                service.GetApplicationsByUserAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(orgApps);

        var result = await _controller.GetApplicationsByUser(10, "user-1", CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<IEnumerable<OrganisationApplicationResponse>>>()
            .Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<OrganisationApplicationResponse>>().Subject
            .ToList();
        response.Should().HaveCount(1);
        response[0].ApplicationId.Should().Be(30);
    }

    [Fact]
    public async Task EnableApplication_WhenValid_ReturnsCreated()
    {
        var request = new EnableApplicationRequest { ApplicationId = 5 };
        var orgApp = new OrganisationApplication
        {
            Id = 3,
            OrganisationId = 10,
            ApplicationId = 5,
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero),
            CreatedBy = "system"
        };
        _organisationApplicationService.Setup(service => service.EnableApplicationAsync(
                10,
                request.ApplicationId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(orgApp);

        var result = await _controller.EnableApplication(10, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<OrganisationApplicationResponse>>().Subject;
        var created = actionResult.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(OrganisationApplicationsController.GetApplications));
        created.RouteValues.Should().ContainKey("orgId").WhoseValue.Should().Be(10);
        var response = created.Value.Should().BeOfType<OrganisationApplicationResponse>().Subject;
        response.ApplicationId.Should().Be(5);
    }

    [Fact]
    public async Task EnableApplication_WhenNotFound_ReturnsNotFound()
    {
        var request = new EnableApplicationRequest { ApplicationId = 5 };
        _organisationApplicationService.Setup(service => service.EnableApplicationAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Organisation", 10));

        var result = await _controller.EnableApplication(10, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<OrganisationApplicationResponse>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task EnableApplication_WhenDuplicate_ReturnsBadRequest()
    {
        var request = new EnableApplicationRequest { ApplicationId = 5 };
        _organisationApplicationService.Setup(service => service.EnableApplicationAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateEntityException("OrganisationApplication", "ApplicationId", 5));

        var result = await _controller.EnableApplication(10, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<OrganisationApplicationResponse>>().Subject;
        var badRequest = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var error = badRequest.Value.Should().BeOfType<ErrorResponse>().Subject;
        error.Code.Should().Be("DUPLICATE_ENTITY");
    }

    [Fact]
    public async Task DisableApplication_WhenValid_ReturnsNoContent()
    {
        var result = await _controller.DisableApplication(10, 5, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _organisationApplicationService.Verify(
            service => service.DisableApplicationAsync(10, 5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DisableApplication_WhenNotFound_ReturnsNotFound()
    {
        _organisationApplicationService
            .Setup(service => service.DisableApplicationAsync(10, 5, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("OrganisationApplication", 5));

        var result = await _controller.DisableApplication(10, 5, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
