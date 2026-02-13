using CO.CDP.UserManagement.Api.Controllers;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.Models;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.UserManagement.Api.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IPersonLookupService> _personLookupService;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _personLookupService = new Mock<IPersonLookupService>();
        var logger = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(_personLookupService.Object, logger.Object);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task LookupUser_WhenUserPrincipalIdMissing_ReturnsBadRequest(string? userPrincipalId)
    {
        var result = await _controller.LookupUser(userPrincipalId!, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<PersonDetailsResponse>>().Subject;
        var badRequest = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var error = badRequest.Value.Should().BeOfType<ErrorResponse>().Subject;
        error.Message.Should().Be("User principal ID is required.");
        _personLookupService.Verify(service => service.GetPersonDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LookupUser_WhenNotFound_ReturnsNotFound()
    {
        _personLookupService.Setup(service => service.GetPersonDetailsAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((PersonDetails?)null);

        var result = await _controller.LookupUser("user-1", CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<PersonDetailsResponse>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task LookupUser_WhenValid_ReturnsOk()
    {
        var person = new PersonDetails
        {
            CdpPersonId = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com"
        };
        _personLookupService.Setup(service => service.GetPersonDetailsAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(person);

        var result = await _controller.LookupUser("user-1", CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<PersonDetailsResponse>>().Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<PersonDetailsResponse>().Subject;
        response.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task LookupUser_WhenLookupFails_ReturnsServiceUnavailable()
    {
        _personLookupService.Setup(service => service.GetPersonDetailsAsync("user-1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PersonLookupException("Service unavailable"));

        var result = await _controller.LookupUser("user-1", CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<PersonDetailsResponse>>().Subject;
        var objectResult = actionResult.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
        objectResult.Value.Should().BeOfType<ErrorResponse>();
    }
}
