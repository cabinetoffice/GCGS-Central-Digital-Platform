using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.Models;
using CO.CDP.UserManagement.Infrastructure.Services;
using CO.CDP.UserManagement.Shared.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using CoreOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Api.Tests.Services;

public class JoinRequestOrchestrationServiceTests
{
    private const string ReviewerPrincipalId = "urn:fdc:gov.uk:2022:abc123";

    private readonly Guid _cdpOrgId = Guid.NewGuid();
    private readonly Guid _joinRequestId = Guid.NewGuid();
    private readonly Mock<IUserOrganisationMembershipRepository> _membershipRepository = new();
    private readonly Mock<IOrganisationApiAdapter> _organisationApiAdapter = new();
    private readonly Mock<IOrganisationRepository> _organisationRepository = new();
    private readonly Mock<IPersonApiAdapter> _personApiAdapter = new();
    private readonly Guid _requestingPersonId = Guid.NewGuid();
    private readonly Guid _reviewerPersonGuid = Guid.NewGuid();
    private readonly Mock<IRoleMappingService> _roleMappingService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private CoreOrganisation _organisation = null!;

    public JoinRequestOrchestrationServiceTests()
    {
        _organisation = new CoreOrganisation
        {
            Id = 42,
            CdpOrganisationGuid = _cdpOrgId,
            Name = "Test Org",
            Slug = "test-org"
        };

        _organisationRepository
            .Setup(r => r.GetByCdpGuidAsync(_cdpOrgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_organisation);

        _personApiAdapter
            .Setup(a => a.GetPersonDetailsAsync(ReviewerPrincipalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonDetails
            {
                CdpPersonId = _reviewerPersonGuid, FirstName = "Reviewer", LastName = "User",
                Email = "reviewer@example.com"
            });

        _membershipRepository
            .Setup(r => r.ExistsByPersonIdAndOrganisationAsync(
                It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _roleMappingService
            .Setup(r => r.ApplyRoleDefinitionAsync(
                It.IsAny<UserOrganisationMembership>(), It.IsAny<OrganisationRole>(), It.IsAny<CancellationToken>()))
            .Returns<UserOrganisationMembership, OrganisationRole, CancellationToken>((m, role, _) =>
            {
                m.OrganisationRoleId = (int)role;
                return Task.CompletedTask;
            });

        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    private JoinRequestOrchestrationService CreateSut() =>
        new(_organisationRepository.Object,
            _organisationApiAdapter.Object,
            _personApiAdapter.Object,
            _membershipRepository.Object,
            _roleMappingService.Object,
            _unitOfWork.Object,
            new Mock<ILogger<JoinRequestOrchestrationService>>().Object);

    // ── ApproveJoinRequestAsync ────────────────────────────────────────────────

    [Fact]
    public async Task ApproveJoinRequestAsync_CallsOiApiWithAcceptedStatus()
    {
        var sut = CreateSut();

        await sut.ApproveJoinRequestAsync(
            _cdpOrgId, _joinRequestId, _requestingPersonId, ReviewerPrincipalId);

        _organisationApiAdapter.Verify(a =>
                a.UpdateJoinRequestAsync(
                    _cdpOrgId, _joinRequestId, "Accepted", _reviewerPersonGuid, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ApproveJoinRequestAsync_CreatesUmMembershipWithMemberRole()
    {
        UserOrganisationMembership? createdMembership = null;
        _membershipRepository
            .Setup(r => r.Add(It.IsAny<UserOrganisationMembership>()))
            .Callback<UserOrganisationMembership>(m => createdMembership = m);

        var sut = CreateSut();

        await sut.ApproveJoinRequestAsync(
            _cdpOrgId, _joinRequestId, _requestingPersonId, ReviewerPrincipalId);

        createdMembership.Should().NotBeNull();
        createdMembership!.CdpPersonId.Should().Be(_requestingPersonId);
        createdMembership.OrganisationId.Should().Be(_organisation.Id);
        createdMembership.IsActive.Should().BeTrue();
        createdMembership.OrganisationRoleId.Should().Be((int)OrganisationRole.Member);
        createdMembership.InvitedBy.Should().Be(_reviewerPersonGuid.ToString());
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveJoinRequestAsync_WhenOrganisationNotFound_ThrowsEntityNotFoundException()
    {
        _organisationRepository
            .Setup(r => r.GetByCdpGuidAsync(_cdpOrgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CoreOrganisation?)null);

        var sut = CreateSut();

        var act = () => sut.ApproveJoinRequestAsync(
            _cdpOrgId, _joinRequestId, _requestingPersonId, ReviewerPrincipalId);

        await act.Should().ThrowAsync<EntityNotFoundException>()
            .WithMessage($"*{_cdpOrgId}*");
    }

    [Fact]
    public async Task ApproveJoinRequestAsync_WhenReviewerPersonNotFound_ThrowsEntityNotFoundException()
    {
        _personApiAdapter
            .Setup(a => a.GetPersonDetailsAsync(ReviewerPrincipalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PersonDetails?)null);

        var sut = CreateSut();

        var act = () => sut.ApproveJoinRequestAsync(
            _cdpOrgId, _joinRequestId, _requestingPersonId, ReviewerPrincipalId);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ApproveJoinRequestAsync_WhenOiApiFails_DoesNotCreateMembership()
    {
        _organisationApiAdapter
            .Setup(a => a.UpdateJoinRequestAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(),
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("OI API unavailable"));

        var sut = CreateSut();

        var act = () => sut.ApproveJoinRequestAsync(
            _cdpOrgId, _joinRequestId, _requestingPersonId, ReviewerPrincipalId);

        await act.Should().ThrowAsync<HttpRequestException>();
        _membershipRepository.Verify(r => r.Add(It.IsAny<UserOrganisationMembership>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApproveJoinRequestAsync_WhenMembershipAlreadyExists_SkipsCreation()
    {
        _membershipRepository
            .Setup(r => r.ExistsByPersonIdAndOrganisationAsync(
                _requestingPersonId, _organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = CreateSut();

        await sut.ApproveJoinRequestAsync(
            _cdpOrgId, _joinRequestId, _requestingPersonId, ReviewerPrincipalId);

        _membershipRepository.Verify(r => r.Add(It.IsAny<UserOrganisationMembership>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── RejectJoinRequestAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task RejectJoinRequestAsync_CallsOiApiWithRejectedStatus()
    {
        var sut = CreateSut();

        await sut.RejectJoinRequestAsync(
            _cdpOrgId, _joinRequestId, _requestingPersonId, ReviewerPrincipalId);

        _organisationApiAdapter.Verify(a =>
                a.UpdateJoinRequestAsync(
                    _cdpOrgId, _joinRequestId, "Rejected", _reviewerPersonGuid, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RejectJoinRequestAsync_NeverCreatesMembership()
    {
        var sut = CreateSut();

        await sut.RejectJoinRequestAsync(
            _cdpOrgId, _joinRequestId, _requestingPersonId, ReviewerPrincipalId);

        _membershipRepository.Verify(r => r.Add(It.IsAny<UserOrganisationMembership>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RejectJoinRequestAsync_WhenOiApiFails_Throws()
    {
        _organisationApiAdapter
            .Setup(a => a.UpdateJoinRequestAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(),
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("OI API unavailable"));

        var sut = CreateSut();

        var act = () => sut.RejectJoinRequestAsync(
            _cdpOrgId, _joinRequestId, _requestingPersonId, ReviewerPrincipalId);

        await act.Should().ThrowAsync<HttpRequestException>();
    }
}