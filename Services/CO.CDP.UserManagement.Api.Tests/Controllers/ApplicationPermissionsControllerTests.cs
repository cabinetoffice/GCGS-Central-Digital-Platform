using CO.CDP.UserManagement.Api.Controllers;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.UserManagement.UnitTests.Controllers;

public class ApplicationPermissionsControllerTests
{
    private readonly Mock<IPermissionService> _permissionService;
    private readonly ApplicationPermissionsController _controller;

    public ApplicationPermissionsControllerTests()
    {
        _permissionService = new Mock<IPermissionService>();
        _controller = new ApplicationPermissionsController(_permissionService.Object);
    }

    [Fact]
    public async Task GetPermissions_WhenApplicationExists_ReturnsOk()
    {
        var permissions = new List<ApplicationPermission>
        {
            new()
            {
                Id = 1,
                ApplicationId = 10,
                Name = "Read",
                IsActive = true,
                CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                CreatedBy = "system"
            }
        };
        _permissionService.Setup(service => service.GetByApplicationIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var result = await _controller.GetPermissions(10, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<IEnumerable<PermissionResponse>>>().Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<PermissionResponse>>().Subject.ToList();
        response.Should().HaveCount(1);
        response[0].Name.Should().Be("Read");
    }

    [Fact]
    public async Task GetPermissions_WhenNotFound_ReturnsNotFound()
    {
        _permissionService.Setup(service => service.GetByApplicationIdAsync(10, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Application", 10));

        var result = await _controller.GetPermissions(10, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<IEnumerable<PermissionResponse>>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreatePermission_WhenValid_ReturnsCreated()
    {
        var request = new CreatePermissionRequest
        {
            Name = "Write",
            Description = "Can write",
            IsActive = true
        };
        var permission = new ApplicationPermission
        {
            Id = 2,
            ApplicationId = 5,
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero),
            CreatedBy = "system"
        };
        _permissionService.Setup(service => service.CreateAsync(
                5,
                request.Name,
                request.Description,
                request.IsActive,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(permission);

        var result = await _controller.CreatePermission(5, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<PermissionResponse>>().Subject;
        var created = actionResult.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(ApplicationPermissionsController.GetPermissions));
        created.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(5);
        var response = created.Value.Should().BeOfType<PermissionResponse>().Subject;
        response.Id.Should().Be(2);
    }

    [Fact]
    public async Task CreatePermission_WhenNotFound_ReturnsNotFound()
    {
        var request = new CreatePermissionRequest
        {
            Name = "Write",
            IsActive = true
        };
        _permissionService.Setup(service => service.CreateAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Application", 5));

        var result = await _controller.CreatePermission(5, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<PermissionResponse>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreatePermission_WhenDuplicate_ReturnsBadRequest()
    {
        var request = new CreatePermissionRequest
        {
            Name = "Write",
            IsActive = true
        };
        _permissionService.Setup(service => service.CreateAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateEntityException("Permission", "Name", "Write"));

        var result = await _controller.CreatePermission(5, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<PermissionResponse>>().Subject;
        var badRequest = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var error = badRequest.Value.Should().BeOfType<ErrorResponse>().Subject;
        error.Code.Should().Be("DUPLICATE_ENTITY");
    }

    [Fact]
    public async Task UpdatePermission_WhenValid_ReturnsOk()
    {
        var request = new UpdatePermissionRequest
        {
            Name = "Updated",
            Description = "Updated",
            IsActive = true
        };
        var permission = new ApplicationPermission
        {
            Id = 7,
            ApplicationId = 9,
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero),
            CreatedBy = "system"
        };
        _permissionService.Setup(service => service.UpdateAsync(
                7,
                request.Name,
                request.Description,
                request.IsActive,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(permission);

        var result = await _controller.UpdatePermission(9, 7, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<PermissionResponse>>().Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<PermissionResponse>().Subject;
        response.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdatePermission_WhenNotFound_ReturnsNotFound()
    {
        var request = new UpdatePermissionRequest
        {
            Name = "Updated",
            IsActive = true
        };
        _permissionService.Setup(service => service.UpdateAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Permission", 7));

        var result = await _controller.UpdatePermission(9, 7, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<PermissionResponse>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdatePermission_WhenDuplicate_ReturnsBadRequest()
    {
        var request = new UpdatePermissionRequest
        {
            Name = "Updated",
            IsActive = true
        };
        _permissionService.Setup(service => service.UpdateAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateEntityException("Permission", "Name", "Updated"));

        var result = await _controller.UpdatePermission(9, 7, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<PermissionResponse>>().Subject;
        var badRequest = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var error = badRequest.Value.Should().BeOfType<ErrorResponse>().Subject;
        error.Code.Should().Be("DUPLICATE_ENTITY");
    }

    [Fact]
    public async Task DeletePermission_WhenValid_ReturnsNoContent()
    {
        var result = await _controller.DeletePermission(4, 11, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _permissionService.Verify(service => service.DeleteAsync(11, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeletePermission_WhenNotFound_ReturnsNotFound()
    {
        _permissionService.Setup(service => service.DeleteAsync(11, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Permission", 11));

        var result = await _controller.DeletePermission(4, 11, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}