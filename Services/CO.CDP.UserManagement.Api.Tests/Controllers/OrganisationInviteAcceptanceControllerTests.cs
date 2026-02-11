using CO.CDP.UserManagement.Api.Controllers;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.UserManagement.UnitTests.Controllers;

public class OrganisationInviteAcceptanceControllerTests
{
    private readonly Mock<IInviteOrchestrationService> _inviteOrchestrationService;
    private readonly Mock<ILogger<OrganisationInviteAcceptanceController>> _logger;

    public OrganisationInviteAcceptanceControllerTests()
    {
        _inviteOrchestrationService = new Mock<IInviteOrchestrationService>();
        _logger = new Mock<ILogger<OrganisationInviteAcceptanceController>>();
    }

    [Fact]
    public async Task AcceptInvite_WhenInternalFlowDisabled_ReturnsServiceUnavailable()
    {
        var controller = new OrganisationInviteAcceptanceController(
            _inviteOrchestrationService.Object,
            BuildConfiguration(false),
            _logger.Object);

        var request = new AcceptOrganisationInviteRequest
        {
            UserPrincipalId = "user-1",
            CdpPersonId = Guid.NewGuid()
        };

        var result = await controller.AcceptInvite(Guid.NewGuid(), 1, request, CancellationToken.None);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
        var error = objectResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        error.Message.Should().Be("Internal invite flow is disabled.");
        _inviteOrchestrationService.Verify(service => service.AcceptInviteAsync(
            It.IsAny<Guid>(),
            It.IsAny<int>(),
            It.IsAny<AcceptOrganisationInviteRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AcceptInvite_WhenEnabled_ReturnsNoContent()
    {
        var controller = new OrganisationInviteAcceptanceController(
            _inviteOrchestrationService.Object,
            BuildConfiguration(true),
            _logger.Object);

        var request = new AcceptOrganisationInviteRequest
        {
            UserPrincipalId = "user-1",
            CdpPersonId = Guid.NewGuid()
        };

        var result = await controller.AcceptInvite(Guid.NewGuid(), 1, request, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _inviteOrchestrationService.Verify(service => service.AcceptInviteAsync(
            It.IsAny<Guid>(),
            It.IsAny<int>(),
            request,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AcceptInvite_WhenNotFound_ReturnsNotFound()
    {
        _inviteOrchestrationService.Setup(service => service.AcceptInviteAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<AcceptOrganisationInviteRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("PendingOrganisationInvite", 1));

        var controller = new OrganisationInviteAcceptanceController(
            _inviteOrchestrationService.Object,
            BuildConfiguration(true),
            _logger.Object);

        var request = new AcceptOrganisationInviteRequest
        {
            UserPrincipalId = "user-1",
            CdpPersonId = Guid.NewGuid()
        };

        var result = await controller.AcceptInvite(Guid.NewGuid(), 1, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        actionResult.Value.Should().BeOfType<ErrorResponse>();
    }

    private static IConfiguration BuildConfiguration(bool enabled)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Features:InternalInviteFlowEnabled"] = enabled.ToString()
            })
            .Build();
    }
}
