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

namespace CO.CDP.UserManagement.Api.Tests.Controllers;

public class ApplicationsControllerTests
{
    private readonly Mock<IApplicationService> _applicationService;
    private readonly ApplicationsController _controller;

    public ApplicationsControllerTests()
    {
        _applicationService = new Mock<IApplicationService>();
        var logger = new Mock<ILogger<ApplicationsController>>();
        _controller = new ApplicationsController(_applicationService.Object, logger.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithApplications()
    {
        var applications = new List<Application>
        {
            new()
            {
                Id = 1,
                Name = "App One",
                ClientId = "app-one",
                Description = "First",
                Category = "Category",
                IsActive = true,
                CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
            },
            new()
            {
                Id = 2,
                Name = "App Two",
                ClientId = "app-two",
                IsActive = false,
                CreatedAt = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero)
            }
        };
        _applicationService.Setup(service => service.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(applications);

        var result = await _controller.GetAll(CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<IEnumerable<ApplicationResponse>>>().Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<ApplicationResponse>>().Subject.ToList();
        response.Should().HaveCount(2);
        response.Select(r => r.Id).Should().BeEquivalentTo(new[] { 1, 2 });
        response[0].ClientId.Should().Be("app-one");
    }

    [Fact]
    public async Task GetById_WhenApplicationExists_ReturnsOk()
    {
        var application = new Application
        {
            Id = 3,
            Name = "App",
            ClientId = "client",
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero)
        };
        _applicationService.Setup(service => service.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        var result = await _controller.GetById(3, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<ApplicationResponse>>().Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApplicationResponse>().Subject;
        response.Id.Should().Be(3);
        response.Name.Should().Be("App");
    }

    [Fact]
    public async Task GetById_WhenApplicationDoesNotExist_ReturnsNotFound()
    {
        _applicationService.Setup(service => service.GetByIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Application?)null);

        var result = await _controller.GetById(42, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<ApplicationResponse>>().Subject;
        var notFoundResult = actionResult.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var error = notFoundResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        error.Message.Should().Be("Application with ID 42 not found.");
    }

    [Fact]
    public async Task Create_WhenValid_ReturnsCreated()
    {
        var request = new CreateApplicationRequest
        {
            Name = "App",
            ClientId = "client",
            Description = "Description",
            IsActive = true
        };
        var application = new Application
        {
            Id = 10,
            Name = request.Name,
            ClientId = request.ClientId,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = new DateTimeOffset(2024, 4, 1, 0, 0, 0, TimeSpan.Zero)
        };
        _applicationService.Setup(service => service.CreateAsync(
                request.Name,
                request.ClientId,
                request.Description,
                request.IsActive,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        var result = await _controller.Create(request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<ApplicationResponse>>().Subject;
        var created = actionResult.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(ApplicationsController.GetById));
        created.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(10);
        var response = created.Value.Should().BeOfType<ApplicationResponse>().Subject;
        response.ClientId.Should().Be("client");
    }

    [Fact]
    public async Task Create_WhenDuplicate_ReturnsBadRequest()
    {
        var request = new CreateApplicationRequest
        {
            Name = "App",
            ClientId = "duplicate",
            IsActive = true
        };
        _applicationService.Setup(service => service.CreateAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateEntityException("Application", "ClientId", "duplicate"));

        var result = await _controller.Create(request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<ApplicationResponse>>().Subject;
        var badRequest = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var error = badRequest.Value.Should().BeOfType<ErrorResponse>().Subject;
        error.Code.Should().Be("DUPLICATE_ENTITY");
    }

    [Fact]
    public async Task Update_WhenValid_ReturnsOk()
    {
        var request = new UpdateApplicationRequest
        {
            Name = "Updated",
            Description = "Updated description",
            IsActive = false
        };
        var application = new Application
        {
            Id = 7,
            Name = request.Name,
            ClientId = "client",
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = new DateTimeOffset(2024, 5, 1, 0, 0, 0, TimeSpan.Zero)
        };
        _applicationService.Setup(service => service.UpdateAsync(
                7,
                request.Name,
                request.Description,
                request.IsActive,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        var result = await _controller.Update(7, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<ApplicationResponse>>().Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApplicationResponse>().Subject;
        response.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task Update_WhenNotFound_ReturnsNotFound()
    {
        var request = new UpdateApplicationRequest
        {
            Name = "Updated",
            IsActive = true
        };
        _applicationService.Setup(service => service.UpdateAsync(
                99,
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Application", 99));

        var result = await _controller.Update(99, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<ApplicationResponse>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_WhenValid_ReturnsNoContent()
    {
        var result = await _controller.Delete(8, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _applicationService.Verify(service => service.DeleteAsync(8, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenNotFound_ReturnsNotFound()
    {
        _applicationService.Setup(service => service.DeleteAsync(15, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Application", 15));

        var result = await _controller.Delete(15, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}