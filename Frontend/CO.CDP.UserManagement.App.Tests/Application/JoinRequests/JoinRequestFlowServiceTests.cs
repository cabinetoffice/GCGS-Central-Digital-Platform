using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Adapters;
using CO.CDP.UserManagement.App.Application.JoinRequests.Implementations;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Moq;

namespace CO.CDP.UserManagement.App.Tests.Application.JoinRequests;

public class JoinRequestFlowServiceTests
{
    private readonly Mock<IUserManagementApiAdapter> _adapter = new();
    private readonly Guid _cdpOrgId = Guid.NewGuid();
    private readonly Guid _joinRequestId = Guid.NewGuid();
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _personId = Guid.NewGuid();

    private JoinRequestFlowService CreateSut() => new(_adapter.Object);

    private OrganisationResponse MakeOrg() => new()
    {
        Id = 1,
        CdpOrganisationGuid = _cdpOrgId,
        Name = "Test Org",
        Slug = "test-org",
        IsActive = true,
        CreatedAt = DateTimeOffset.UtcNow
    };

    private List<JoinRequestResponse> MakeJoinRequests() =>
    [
        new JoinRequestResponse
        {
            Id = _joinRequestId,
            PersonId = _personId,
            FirstName = "Alice",
            LastName = "Brown",
            Email = "alice@example.com"
        }
    ];

    // ── GetConfirmViewModelAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetConfirmViewModelAsync_WhenRequestFound_ReturnsViewModel()
    {
        _adapter.Setup(a => a.GetOrganisationByGuidAsync(_organisationId, default)).ReturnsAsync(MakeOrg());
        _adapter.Setup(a => a.GetJoinRequestsAsync(_cdpOrgId, default)).ReturnsAsync(MakeJoinRequests());

        var vm = await CreateSut().GetConfirmViewModelAsync(
            _organisationId, _joinRequestId, _personId, JoinRequestAction.Approve);

        vm.Should().NotBeNull();
        vm!.FullName.Should().Be("Alice Brown");
        vm.Email.Should().Be("alice@example.com");
        vm.Action.Should().Be(JoinRequestAction.Approve);
    }

    [Fact]
    public async Task GetConfirmViewModelAsync_WhenOrgNotFound_ReturnsNull()
    {
        _adapter.Setup(a => a.GetOrganisationByGuidAsync(_organisationId, default))
            .ReturnsAsync((OrganisationResponse?)null);

        var vm = await CreateSut().GetConfirmViewModelAsync(
            _organisationId, _joinRequestId, _personId, JoinRequestAction.Approve);

        vm.Should().BeNull();
    }

    [Fact]
    public async Task GetConfirmViewModelAsync_WhenJoinRequestNotFound_ReturnsNull()
    {
        _adapter.Setup(a => a.GetOrganisationByGuidAsync(_organisationId, default)).ReturnsAsync(MakeOrg());
        _adapter.Setup(a => a.GetJoinRequestsAsync(_cdpOrgId, default))
            .ReturnsAsync(new List<JoinRequestResponse>());

        var vm = await CreateSut().GetConfirmViewModelAsync(
            _organisationId, _joinRequestId, _personId, JoinRequestAction.Approve);

        vm.Should().BeNull();
    }

    // ── ApproveAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ApproveAsync_WhenSuccess_ReturnsSuccess()
    {
        _adapter.Setup(a => a.GetOrganisationByGuidAsync(_organisationId, default)).ReturnsAsync(MakeOrg());
        _adapter.Setup(a => a.GetJoinRequestsAsync(_cdpOrgId, default)).ReturnsAsync(MakeJoinRequests());
        _adapter.Setup(a => a.ReviewJoinRequestAsync(
                _cdpOrgId, _joinRequestId,
                It.Is<ReviewJoinRequestRequest>(r => r.Decision == JoinRequestDecision.Accepted
                                                     && r.RequestingPersonId == _personId),
                default))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));

        var result = await CreateSut().ApproveAsync(_organisationId, _joinRequestId, _personId);

        result.IsSuccess.Should().BeTrue();
        result.Match(_ => ServiceOutcome.Success, o => o).Should().Be(ServiceOutcome.Success);
    }

    [Fact]
    public async Task ApproveAsync_WhenOrgNotFound_ReturnsNotFound()
    {
        _adapter.Setup(a => a.GetOrganisationByGuidAsync(_organisationId, default))
            .ReturnsAsync((OrganisationResponse?)null);

        var result = await CreateSut().ApproveAsync(_organisationId, _joinRequestId, _personId);

        result.IsSuccess.Should().BeTrue();
        result.Match(_ => ServiceOutcome.Success, o => o).Should().Be(ServiceOutcome.NotFound);
    }

    [Fact]
    public async Task ApproveAsync_WhenAdapterFails_ReturnsFailure()
    {
        _adapter.Setup(a => a.GetOrganisationByGuidAsync(_organisationId, default)).ReturnsAsync(MakeOrg());
        _adapter.Setup(a => a.GetJoinRequestsAsync(_cdpOrgId, default)).ReturnsAsync(MakeJoinRequests());
        _adapter.Setup(a => a.ReviewJoinRequestAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<ReviewJoinRequestRequest>(), default))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Failure(ServiceFailure.Unexpected));

        var result = await CreateSut().ApproveAsync(_organisationId, _joinRequestId, _personId);

        result.IsFailure.Should().BeTrue();
    }

    // ── RejectAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task RejectAsync_WhenSuccess_ReturnsSuccess()
    {
        _adapter.Setup(a => a.GetOrganisationByGuidAsync(_organisationId, default)).ReturnsAsync(MakeOrg());
        _adapter.Setup(a => a.GetJoinRequestsAsync(_cdpOrgId, default)).ReturnsAsync(MakeJoinRequests());
        _adapter.Setup(a => a.ReviewJoinRequestAsync(
                _cdpOrgId, _joinRequestId,
                It.Is<ReviewJoinRequestRequest>(r => r.Decision == JoinRequestDecision.Rejected
                                                     && r.RequestingPersonId == _personId),
                default))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));

        var result = await CreateSut().RejectAsync(_organisationId, _joinRequestId, _personId);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RejectAsync_WhenOrgNotFound_ReturnsNotFound()
    {
        _adapter.Setup(a => a.GetOrganisationByGuidAsync(_organisationId, default))
            .ReturnsAsync((OrganisationResponse?)null);

        var result = await CreateSut().RejectAsync(_organisationId, _joinRequestId, _personId);

        result.Match(_ => ServiceOutcome.Success, o => o).Should().Be(ServiceOutcome.NotFound);
    }
}