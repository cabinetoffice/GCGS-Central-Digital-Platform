using CO.CDP.UserManagement.Api.Controllers;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SystemInvalidOperationException = System.InvalidOperationException;

namespace CO.CDP.UserManagement.Api.Tests.Controllers;

public class ApplicationRolesControllerTests
{
    private readonly Mock<IRoleService> _roleService;
    private readonly ApplicationRolesController _controller;

    public ApplicationRolesControllerTests()
    {
        _roleService = new Mock<IRoleService>();
        _controller = new ApplicationRolesController(_roleService.Object);
    }

    [Fact]
    public async Task GetRoles_WhenApplicationExists_ReturnsOk()
    {
        var roles = new List<ApplicationRole>
        {
            new()
            {
                Id = 1,
                ApplicationId = 3,
                Name = "Admin",
                IsActive = true,
                CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                CreatedBy = "system"
            }
        };
        _roleService.Setup(service => service.GetByApplicationIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        var result = await _controller.GetRoles(3, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<IEnumerable<RoleResponse>>>().Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<RoleResponse>>().Subject.ToList();
        response.Should().HaveCount(1);
        response[0].Name.Should().Be("Admin");
    }

    [Fact]
    public async Task GetRoles_WhenNotFound_ReturnsNotFound()
    {
        _roleService.Setup(service => service.GetByApplicationIdAsync(3, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Application", 3));

        var result = await _controller.GetRoles(3, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<IEnumerable<RoleResponse>>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateRole_WhenValid_ReturnsCreated()
    {
        var request = new CreateRoleRequest
        {
            Name = "Editor",
            Description = "Can edit",
            IsActive = true
        };
        var role = new ApplicationRole
        {
            Id = 4,
            ApplicationId = 3,
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero),
            CreatedBy = "system"
        };
        _roleService.Setup(service => service.CreateAsync(
                3,
                request.Name,
                request.Description,
                request.IsActive,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        var result = await _controller.CreateRole(3, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<RoleResponse>>().Subject;
        var created = actionResult.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(ApplicationRolesController.GetRoles));
        created.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(3);
        var response = created.Value.Should().BeOfType<RoleResponse>().Subject;
        response.Id.Should().Be(4);
    }

    [Fact]
    public async Task CreateRole_WhenNotFound_ReturnsNotFound()
    {
        var request = new CreateRoleRequest
        {
            Name = "Editor",
            IsActive = true
        };
        _roleService.Setup(service => service.CreateAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Application", 3));

        var result = await _controller.CreateRole(3, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<RoleResponse>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateRole_WhenDuplicate_ReturnsBadRequest()
    {
        var request = new CreateRoleRequest
        {
            Name = "Editor",
            IsActive = true
        };
        _roleService.Setup(service => service.CreateAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateEntityException("Role", "Name", "Editor"));

        var result = await _controller.CreateRole(3, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<RoleResponse>>().Subject;
        var badRequest = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var error = badRequest.Value.Should().BeOfType<ErrorResponse>().Subject;
        error.Code.Should().Be("DUPLICATE_ENTITY");
    }

    [Fact]
    public async Task UpdateRole_WhenValid_ReturnsOk()
    {
        var request = new UpdateRoleRequest
        {
            Name = "Updated",
            Description = "Updated",
            IsActive = false
        };
        var role = new ApplicationRole
        {
            Id = 6,
            ApplicationId = 3,
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero),
            CreatedBy = "system"
        };
        _roleService.Setup(service => service.UpdateAsync(
                6,
                request.Name,
                request.Description,
                request.IsActive,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        var result = await _controller.UpdateRole(3, 6, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<RoleResponse>>().Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<RoleResponse>().Subject;
        response.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateRole_WhenNotFound_ReturnsNotFound()
    {
        var request = new UpdateRoleRequest
        {
            Name = "Updated",
            IsActive = true
        };
        _roleService.Setup(service => service.UpdateAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Role", 6));

        var result = await _controller.UpdateRole(3, 6, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<RoleResponse>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateRole_WhenDuplicate_ReturnsBadRequest()
    {
        var request = new UpdateRoleRequest
        {
            Name = "Updated",
            IsActive = true
        };
        _roleService.Setup(service => service.UpdateAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateEntityException("Role", "Name", "Updated"));

        var result = await _controller.UpdateRole(3, 6, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<RoleResponse>>().Subject;
        var badRequest = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var error = badRequest.Value.Should().BeOfType<ErrorResponse>().Subject;
        error.Code.Should().Be("DUPLICATE_ENTITY");
    }

    [Fact]
    public async Task DeleteRole_WhenValid_ReturnsNoContent()
    {
        var result = await _controller.DeleteRole(3, 9, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _roleService.Verify(service => service.DeleteAsync(9, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteRole_WhenNotFound_ReturnsNotFound()
    {
        _roleService.Setup(service => service.DeleteAsync(9, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Role", 9));

        var result = await _controller.DeleteRole(3, 9, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AssignPermissions_WhenValid_ReturnsOk()
    {
        var request = new AssignPermissionsRequest
        {
            PermissionIds = new List<int> { 1, 2 }
        };
        var role = new ApplicationRole
        {
            Id = 11,
            ApplicationId = 3,
            Name = "Admin",
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 4, 1, 0, 0, 0, TimeSpan.Zero),
            CreatedBy = "system"
        };
        _roleService.Setup(service => service.AssignPermissionsAsync(
                11,
                request.PermissionIds,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        var result = await _controller.AssignPermissions(3, 11, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<RoleResponse>>().Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<RoleResponse>().Subject;
        response.Id.Should().Be(11);
    }

    [Fact]
    public async Task AssignPermissions_WhenInvalidOperation_ReturnsBadRequest()
    {
        var request = new AssignPermissionsRequest
        {
            PermissionIds = new List<int> { 1 }
        };
        _roleService.Setup(service => service.AssignPermissionsAsync(
                It.IsAny<int>(),
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SystemInvalidOperationException("Invalid"));

        var result = await _controller.AssignPermissions(3, 11, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<RoleResponse>>().Subject;
        var badRequest = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var error = badRequest.Value.Should().BeOfType<ErrorResponse>().Subject;
        error.Code.Should().Be("INVALID_OPERATION");
    }

    [Fact]
    public async Task AssignPermissions_WhenNotFound_ReturnsNotFound()
    {
        var request = new AssignPermissionsRequest
        {
            PermissionIds = new List<int> { 1 }
        };
        _roleService.Setup(service => service.AssignPermissionsAsync(
                It.IsAny<int>(),
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Role", 11));

        var result = await _controller.AssignPermissions(3, 11, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<RoleResponse>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task RemovePermission_WhenRoleNotFound_ReturnsNotFound()
    {
        _roleService.Setup(service => service.GetByIdAsync(21, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationRole?)null);

        var result = await _controller.RemovePermission(3, 21, 4, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<RoleResponse>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task RemovePermission_WhenValid_ReturnsOk()
    {
        var role = new ApplicationRole
        {
            Id = 21,
            ApplicationId = 3,
            Name = "Admin",
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 5, 1, 0, 0, 0, TimeSpan.Zero),
            CreatedBy = "system",
            Permissions = new List<ApplicationPermission>
            {
                new()
                {
                    Id = 4,
                    ApplicationId = 3,
                    Name = "Read",
                    IsActive = true,
                    CreatedAt = new DateTimeOffset(2024, 5, 1, 0, 0, 0, TimeSpan.Zero),
                    CreatedBy = "system"
                },
                new()
                {
                    Id = 5,
                    ApplicationId = 3,
                    Name = "Write",
                    IsActive = true,
                    CreatedAt = new DateTimeOffset(2024, 5, 1, 0, 0, 0, TimeSpan.Zero),
                    CreatedBy = "system"
                }
            }
        };
        _roleService.Setup(service => service.GetByIdAsync(21, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _roleService.Setup(service => service.AssignPermissionsAsync(
                21,
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        var result = await _controller.RemovePermission(3, 21, 4, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<RoleResponse>>().Subject;
        actionResult.Result.Should().BeOfType<OkObjectResult>();
        _roleService.Verify(service => service.AssignPermissionsAsync(
            21,
            It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 5 })),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}