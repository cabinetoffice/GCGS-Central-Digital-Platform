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
using SystemInvalidOperationException = System.InvalidOperationException;

namespace CO.CDP.UserManagement.Api.Tests.Controllers;

public class UserAssignmentsControllerTests
{
    private readonly Mock<IUserAssignmentService> _userAssignmentService;
    private readonly UserAssignmentsController _controller;

    public UserAssignmentsControllerTests()
    {
        _userAssignmentService = new Mock<IUserAssignmentService>();
        var logger = new Mock<ILogger<UserAssignmentsController>>();
        _controller = new UserAssignmentsController(_userAssignmentService.Object, logger.Object);
    }

    [Fact]
    public async Task GetAssignments_WhenValid_ReturnsOk()
    {
        var assignments = new List<UserApplicationAssignment>
        {
            new()
            {
                Id = 1,
                UserOrganisationMembershipId = 2,
                OrganisationApplicationId = 3,
                IsActive = true,
                CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
            }
        };
        _userAssignmentService.Setup(service => service.GetUserAssignmentsAsync("user-1", 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignments);

        var result = await _controller.GetAssignments(10, "user-1", CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<IEnumerable<UserAssignmentResponse>>>().Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<UserAssignmentResponse>>().Subject.ToList();
        response.Should().HaveCount(1);
        response[0].Id.Should().Be(1);
    }

    [Fact]
    public async Task GetAssignments_WhenNotFound_ReturnsNotFound()
    {
        _userAssignmentService.Setup(service => service.GetUserAssignmentsAsync("user-1", 10, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Organisation", 10));

        var result = await _controller.GetAssignments(10, "user-1", CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<IEnumerable<UserAssignmentResponse>>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AssignUser_WhenValid_ReturnsCreated()
    {
        var request = new AssignUserToApplicationRequest
        {
            ApplicationId = 3,
            RoleIds = new List<int> { 1, 2 }
        };
        var assignment = new UserApplicationAssignment
        {
            Id = 4,
            UserOrganisationMembershipId = 2,
            OrganisationApplicationId = 3,
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero)
        };
        _userAssignmentService.Setup(service => service.AssignUserAsync(
                "user-1",
                10,
                request.ApplicationId,
                request.RoleIds,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        var result = await _controller.AssignUser(10, "user-1", request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<UserAssignmentResponse>>().Subject;
        var created = actionResult.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(UserAssignmentsController.GetAssignments));
        created.RouteValues.Should().ContainKey("orgId").WhoseValue.Should().Be(10);
        var response = created.Value.Should().BeOfType<UserAssignmentResponse>().Subject;
        response.Id.Should().Be(4);
    }

    [Fact]
    public async Task AssignUser_WhenDuplicate_ReturnsBadRequest()
    {
        var request = new AssignUserToApplicationRequest
        {
            ApplicationId = 3,
            RoleIds = new List<int> { 1 }
        };
        _userAssignmentService.Setup(service => service.AssignUserAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateEntityException("UserAssignment", "ApplicationId", 3));

        var result = await _controller.AssignUser(10, "user-1", request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<UserAssignmentResponse>>().Subject;
        var badRequest = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var error = badRequest.Value.Should().BeOfType<ErrorResponse>().Subject;
        error.Code.Should().Be("DUPLICATE_ENTITY");
    }

    [Fact]
    public async Task AssignUser_WhenInvalidOperation_ReturnsBadRequest()
    {
        var request = new AssignUserToApplicationRequest
        {
            ApplicationId = 3,
            RoleIds = new List<int> { 1 }
        };
        _userAssignmentService.Setup(service => service.AssignUserAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SystemInvalidOperationException("Invalid"));

        var result = await _controller.AssignUser(10, "user-1", request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<UserAssignmentResponse>>().Subject;
        var badRequest = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var error = badRequest.Value.Should().BeOfType<ErrorResponse>().Subject;
        error.Code.Should().Be("INVALID_OPERATION");
    }

    [Fact]
    public async Task AssignUser_WhenNotFound_ReturnsNotFound()
    {
        var request = new AssignUserToApplicationRequest
        {
            ApplicationId = 3,
            RoleIds = new List<int> { 1 }
        };
        _userAssignmentService.Setup(service => service.AssignUserAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Organisation", 10));

        var result = await _controller.AssignUser(10, "user-1", request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<UserAssignmentResponse>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateAssignment_WhenValid_ReturnsOk()
    {
        var request = new UpdateAssignmentRolesRequest
        {
            RoleIds = new List<int> { 2 }
        };
        var assignment = new UserApplicationAssignment
        {
            Id = 6,
            UserOrganisationMembershipId = 2,
            OrganisationApplicationId = 3,
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero)
        };
        _userAssignmentService.Setup(service => service.UpdateAssignmentAsync(
                6,
                request.RoleIds,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        var result = await _controller.UpdateAssignment(10, "user-1", 6, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<UserAssignmentResponse>>().Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<UserAssignmentResponse>().Subject;
        response.Id.Should().Be(6);
    }

    [Fact]
    public async Task UpdateAssignment_WhenInvalidOperation_ReturnsBadRequest()
    {
        var request = new UpdateAssignmentRolesRequest
        {
            RoleIds = new List<int> { 2 }
        };
        _userAssignmentService.Setup(service => service.UpdateAssignmentAsync(
                It.IsAny<int>(),
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SystemInvalidOperationException("Invalid"));

        var result = await _controller.UpdateAssignment(10, "user-1", 6, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<UserAssignmentResponse>>().Subject;
        var badRequest = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var error = badRequest.Value.Should().BeOfType<ErrorResponse>().Subject;
        error.Code.Should().Be("INVALID_OPERATION");
    }

    [Fact]
    public async Task UpdateAssignment_WhenNotFound_ReturnsNotFound()
    {
        var request = new UpdateAssignmentRolesRequest
        {
            RoleIds = new List<int> { 2 }
        };
        _userAssignmentService.Setup(service => service.UpdateAssignmentAsync(
                It.IsAny<int>(),
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("UserAssignment", 6));

        var result = await _controller.UpdateAssignment(10, "user-1", 6, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<UserAssignmentResponse>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task RevokeAssignment_WhenValid_ReturnsNoContent()
    {
        var result = await _controller.RevokeAssignment(10, "user-1", 7, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _userAssignmentService.Verify(service => service.RevokeAssignmentAsync(7, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeAssignment_WhenNotFound_ReturnsNotFound()
    {
        _userAssignmentService.Setup(service => service.RevokeAssignmentAsync(7, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("UserAssignment", 7));

        var result = await _controller.RevokeAssignment(10, "user-1", 7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
