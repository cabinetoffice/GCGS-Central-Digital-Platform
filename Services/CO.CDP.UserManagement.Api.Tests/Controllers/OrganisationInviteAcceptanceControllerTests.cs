using CO.CDP.UserManagement.Api.Controllers;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.UserManagement.Api.Tests.Controllers;

public class OrganisationInviteAcceptanceControllerTests
{
    private readonly Mock<IInviteOrchestrationService> _inviteOrchestrationService;

    public OrganisationInviteAcceptanceControllerTests()
    {
        _inviteOrchestrationService = new Mock<IInviteOrchestrationService>();
    }

    [Fact]
    public async Task AcceptInvite_WhenEnabled_ReturnsNoContent()
    {
        var controller = new OrganisationInviteAcceptanceController(
            _inviteOrchestrationService.Object);

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
            _inviteOrchestrationService.Object);

        var request = new AcceptOrganisationInviteRequest
        {
            UserPrincipalId = "user-1",
            CdpPersonId = Guid.NewGuid()
        };

        var result = await controller.AcceptInvite(Guid.NewGuid(), 1, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        actionResult.Value.Should().BeOfType<ErrorResponse>();
    }
}
