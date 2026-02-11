using CO.CDP.UserManagement.Api.Controllers;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.Models;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.UserManagement.UnitTests.Controllers;

public class ClaimsControllerTests
{
    private readonly Mock<IClaimsCacheService> _claimsCacheService;
    private readonly ClaimsController _controller;

    public ClaimsControllerTests()
    {
        _claimsCacheService = new Mock<IClaimsCacheService>();
        var logger = new Mock<ILogger<ClaimsController>>();
        _controller = new ClaimsController(_claimsCacheService.Object, logger.Object);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetUserClaims_WhenUserIdMissing_ReturnsBadRequest(string? userId)
    {
        var result = await _controller.GetUserClaims(userId!, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<UserClaims>>().Subject;
        var badRequest = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var error = badRequest.Value.Should().BeOfType<ErrorResponse>().Subject;
        error.Message.Should().Be("User ID is required.");
        _claimsCacheService.Verify(
            service => service.GetUserClaimsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetUserClaims_WhenValid_ReturnsOk()
    {
        var claims = new UserClaims
        {
            UserPrincipalId = "user-1"
        };
        _claimsCacheService.Setup(service => service.GetUserClaimsAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(claims);

        var result = await _controller.GetUserClaims("user-1", CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<UserClaims>>().Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeSameAs(claims);
    }

    [Fact]
    public async Task InvalidateCache_ReturnsNoContent()
    {
        var result = await _controller.InvalidateCache("user-2", CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _claimsCacheService.Verify(service => service.InvalidateCacheAsync("user-2", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}