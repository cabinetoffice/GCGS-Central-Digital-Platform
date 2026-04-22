using CO.CDP.UserManagement.Api.Controllers;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.Models;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.UserManagement.Api.Tests.Controllers;

public class OrganisationJoinRequestsControllerTests
{
    private const string ReviewerPrincipalId = "urn:fdc:gov.uk:2022:abc123";

    private readonly Guid _cdpOrgId = Guid.NewGuid();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly Guid _joinRequestId = Guid.NewGuid();
    private readonly Mock<IJoinRequestOrchestrationService> _orchestrationService = new();
    private readonly Mock<IOrganisationApiAdapter> _organisationApiAdapter = new();
    private readonly Guid _personId = Guid.NewGuid();

    public OrganisationJoinRequestsControllerTests()
    {
        _currentUserService
            .Setup(s => s.GetUserPrincipalId())
            .Returns(ReviewerPrincipalId);
    }

    private OrganisationJoinRequestsController CreateSut() =>
        new(_organisationApiAdapter.Object,
            _orchestrationService.Object,
            _currentUserService.Object,
            new Mock<ILogger<OrganisationJoinRequestsController>>().Object);

    // ── GET join-requests ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetJoinRequests_WhenNoPendingRequests_ReturnsEmptyList()
    {
        _organisationApiAdapter
            .Setup(a => a.GetOrganisationJoinRequestsAsync(_cdpOrgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OiJoinRequest>());

        var sut = CreateSut();

        var result = await sut.GetJoinRequests(_cdpOrgId, CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var responses = ok.Value.Should().BeAssignableTo<IEnumerable<JoinRequestResponse>>().Subject;
        responses.Should().BeEmpty();
    }

    [Fact]
    public async Task GetJoinRequests_WhenPendingRequestsExist_ReturnsMappedResponses()
    {
        var joinRequests = new List<OiJoinRequest>
        {
            new()
            {
                Id = _joinRequestId, PersonId = _personId, FirstName = "Alice", LastName = "Smith",
                Email = "alice@example.com", Status = "Pending"
            }
        };
        _organisationApiAdapter
            .Setup(a => a.GetOrganisationJoinRequestsAsync(_cdpOrgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(joinRequests);

        var sut = CreateSut();

        var result = await sut.GetJoinRequests(_cdpOrgId, CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var responses = ok.Value.Should().BeAssignableTo<IEnumerable<JoinRequestResponse>>().Subject.ToList();
        responses.Should().HaveCount(1);
        responses[0].Id.Should().Be(_joinRequestId);
        responses[0].PersonId.Should().Be(_personId);
        responses[0].FirstName.Should().Be("Alice");
        responses[0].LastName.Should().Be("Smith");
        responses[0].Email.Should().Be("alice@example.com");
    }

    // ── PUT join-requests/{id} ─────────────────────────────────────────────────

    [Fact]
    public async Task ReviewJoinRequest_WhenAccepted_Returns204NoContent()
    {
        var request = new ReviewJoinRequestRequest
        {
            Decision = JoinRequestDecision.Accepted,
            RequestingPersonId = _personId
        };

        var sut = CreateSut();

        var result = await sut.ReviewJoinRequest(_cdpOrgId, _joinRequestId, request, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _orchestrationService.Verify(s =>
                s.ApproveJoinRequestAsync(_cdpOrgId, _joinRequestId, _personId, ReviewerPrincipalId,
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ReviewJoinRequest_WhenRejected_Returns204NoContent()
    {
        var request = new ReviewJoinRequestRequest
        {
            Decision = JoinRequestDecision.Rejected,
            RequestingPersonId = _personId
        };

        var sut = CreateSut();

        var result = await sut.ReviewJoinRequest(_cdpOrgId, _joinRequestId, request, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _orchestrationService.Verify(s =>
                s.RejectJoinRequestAsync(_cdpOrgId, _joinRequestId, _personId, ReviewerPrincipalId,
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ReviewJoinRequest_WhenEntityNotFound_Returns404()
    {
        _orchestrationService
            .Setup(s => s.ApproveJoinRequestAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("JoinRequest", _joinRequestId));

        var request = new ReviewJoinRequestRequest
        {
            Decision = JoinRequestDecision.Accepted,
            RequestingPersonId = _personId
        };

        var sut = CreateSut();

        var result = await sut.ReviewJoinRequest(_cdpOrgId, _joinRequestId, request, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ReviewJoinRequest_WhenUserPrincipalIdMissing_Returns401()
    {
        _currentUserService
            .Setup(s => s.GetUserPrincipalId())
            .Returns(string.Empty);

        var request = new ReviewJoinRequestRequest
        {
            Decision = JoinRequestDecision.Accepted,
            RequestingPersonId = _personId
        };

        var sut = CreateSut();

        var result = await sut.ReviewJoinRequest(_cdpOrgId, _joinRequestId, request, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }
}