using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Services;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using IOrganisationClient = CO.CDP.Organisation.WebApiClient.IOrganisationClient;
using PersonInviteModel = CO.CDP.Organisation.WebApiClient.PersonInviteModel;
using InvitePersonToOrganisation = CO.CDP.Organisation.WebApiClient.InvitePersonToOrganisation;
using CoreOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Api.Tests.Services;

public class InviteOrchestrationServiceTests
{
    private readonly Mock<IOrganisationRepository> _organisationRepositoryMock;
    private readonly Mock<IInviteRoleMappingRepository> _inviteRoleMappingRepositoryMock;
    private readonly Mock<IUserOrganisationMembershipRepository> _membershipRepositoryMock;
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly InviteOrchestrationService _service;

    public InviteOrchestrationServiceTests()
    {
        _organisationRepositoryMock = new Mock<IOrganisationRepository>();
        _inviteRoleMappingRepositoryMock = new Mock<IInviteRoleMappingRepository>();
        _membershipRepositoryMock = new Mock<IUserOrganisationMembershipRepository>();
        _organisationClientMock = new Mock<IOrganisationClient>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<InviteOrchestrationService>>();

        _service = new InviteOrchestrationService(
            _organisationRepositoryMock.Object,
            _inviteRoleMappingRepositoryMock.Object,
            _membershipRepositoryMock.Object,
            _organisationClientMock.Object,
            _unitOfWorkMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task InviteUserAsync_WithValidRequest_CreatesInviteRoleMapping()
    {
        var cdpOrgGuid = Guid.NewGuid();
        var cdpInviteGuid = Guid.NewGuid();
        var organisation = new CoreOrganisation
        {
            Id = 1,
            CdpOrganisationGuid = cdpOrgGuid,
            Name = "Test Org",
            Slug = "test-org"
        };

        var request = new InviteUserRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            OrganisationRole = OrganisationRole.Admin,
            ApplicationAssignments = new List<ApplicationAssignment>()
        };

        var personInviteModel = new PersonInviteModel(
            email: "john.doe@example.com",
            expiresOn: DateTimeOffset.UtcNow.AddDays(7),
            firstName: "John",
            id: cdpInviteGuid,
            lastName: "Doe",
            scopes: new List<string> { "ADMIN" }
        );

        _organisationRepositoryMock
            .Setup(r => r.GetByCdpGuidAsync(cdpOrgGuid, default))
            .ReturnsAsync(organisation);

        _organisationClientMock
            .Setup(c => c.CreatePersonInviteForServiceAsync(
                cdpOrgGuid,
                It.IsAny<InvitePersonToOrganisation>(),
                default))
            .ReturnsAsync(personInviteModel);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);

        var result = await _service.InviteUserAsync(cdpOrgGuid, request, "test-user");

        result.Should().NotBeNull();
        result.CdpPersonInviteGuid.Should().Be(cdpInviteGuid);
        result.OrganisationId.Should().Be(organisation.Id);
        result.OrganisationRole.Should().Be(OrganisationRole.Admin);
        result.CreatedBy.Should().Be("test-user");

        _inviteRoleMappingRepositoryMock.Verify(
            r => r.Add(It.Is<InviteRoleMapping>(m =>
                m.CdpPersonInviteGuid == cdpInviteGuid &&
                m.OrganisationId == organisation.Id &&
                m.OrganisationRole == OrganisationRole.Admin)),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task InviteUserAsync_WhenOrganisationNotFound_ThrowsEntityNotFoundException()
    {
        var cdpOrgGuid = Guid.NewGuid();
        var request = new InviteUserRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            OrganisationRole = OrganisationRole.Admin,
            ApplicationAssignments = new List<ApplicationAssignment>()
        };

        _organisationRepositoryMock
            .Setup(r => r.GetByCdpGuidAsync(cdpOrgGuid, default))
            .ReturnsAsync((CoreOrganisation?)null);

        var act = () => _service.InviteUserAsync(cdpOrgGuid, request, "test-user");

        await act.Should().ThrowAsync<EntityNotFoundException>()
            .WithMessage($"*{cdpOrgGuid}*");
    }

    [Fact]
    public async Task InviteUserAsync_WhenOrganisationClientFails_DoesNotCreateMapping()
    {
        var cdpOrgGuid = Guid.NewGuid();
        var organisation = new CoreOrganisation
        {
            Id = 1,
            CdpOrganisationGuid = cdpOrgGuid,
            Name = "Test Org",
            Slug = "test-org"
        };

        var request = new InviteUserRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            OrganisationRole = OrganisationRole.Admin,
            ApplicationAssignments = new List<ApplicationAssignment>()
        };

        _organisationRepositoryMock
            .Setup(r => r.GetByCdpGuidAsync(cdpOrgGuid, default))
            .ReturnsAsync(organisation);

        _organisationClientMock
            .Setup(c => c.CreatePersonInviteForServiceAsync(
                cdpOrgGuid,
                It.IsAny<InvitePersonToOrganisation>(),
                default))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        var act = () => _service.InviteUserAsync(cdpOrgGuid, request, "test-user");

        await act.Should().ThrowAsync<HttpRequestException>();

        _inviteRoleMappingRepositoryMock.Verify(
            r => r.Add(It.IsAny<InviteRoleMapping>()),
            Times.Never);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task InviteUserAsync_WithApplicationAssignments_CreatesAssignments()
    {
        var cdpOrgGuid = Guid.NewGuid();
        var cdpInviteGuid = Guid.NewGuid();
        var organisation = new CoreOrganisation
        {
            Id = 1,
            CdpOrganisationGuid = cdpOrgGuid,
            Name = "Test Org",
            Slug = "test-org"
        };

        var request = new InviteUserRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            OrganisationRole = OrganisationRole.Admin,
            ApplicationAssignments = new List<ApplicationAssignment>
            {
                new ApplicationAssignment
                {
                    OrganisationApplicationId = 1,
                    ApplicationRoleIds = new List<int> { 1, 2 }
                }
            }
        };

        var personInviteModel = new PersonInviteModel(
            email: "john.doe@example.com",
            expiresOn: DateTimeOffset.UtcNow.AddDays(7),
            firstName: "John",
            id: cdpInviteGuid,
            lastName: "Doe",
            scopes: new List<string> { "ADMIN" }
        );

        _organisationRepositoryMock
            .Setup(r => r.GetByCdpGuidAsync(cdpOrgGuid, default))
            .ReturnsAsync(organisation);

        _organisationClientMock
            .Setup(c => c.CreatePersonInviteForServiceAsync(
                cdpOrgGuid,
                It.IsAny<InvitePersonToOrganisation>(),
                default))
            .ReturnsAsync(personInviteModel);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);

        var result = await _service.InviteUserAsync(cdpOrgGuid, request, "test-user");

        result.ApplicationAssignments.Should().HaveCount(2);
        result.ApplicationAssignments.Should().AllSatisfy(a =>
        {
            a.OrganisationApplicationId.Should().Be(1);
            a.CreatedBy.Should().Be("test-user");
        });
    }

    [Fact]
    public async Task RemoveInviteAsync_WithValidId_DeletesMapping()
    {
        var cdpOrgGuid = Guid.NewGuid();
        var organisation = new CoreOrganisation
        {
            Id = 1,
            CdpOrganisationGuid = cdpOrgGuid,
            Name = "Test Org",
            Slug = "test-org"
        };

        var mapping = new InviteRoleMapping
        {
            Id = 1,
            CdpPersonInviteGuid = Guid.NewGuid(),
            OrganisationId = 1,
            OrganisationRole = OrganisationRole.Admin,
            Organisation = organisation
        };

        _inviteRoleMappingRepositoryMock
            .Setup(r => r.GetByIdAsync(1, default))
            .ReturnsAsync(mapping);

        _organisationRepositoryMock
            .Setup(r => r.GetByCdpGuidAsync(cdpOrgGuid, default))
            .ReturnsAsync(organisation);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);

        await _service.RemoveInviteAsync(cdpOrgGuid, 1);

        _inviteRoleMappingRepositoryMock.Verify(r => r.Remove(mapping), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task RemoveInviteAsync_WhenMappingNotFound_ThrowsEntityNotFoundException()
    {
        var cdpOrgGuid = Guid.NewGuid();
        _inviteRoleMappingRepositoryMock
            .Setup(r => r.GetByIdAsync(999, default))
            .ReturnsAsync((InviteRoleMapping?)null);

        var act = () => _service.RemoveInviteAsync(cdpOrgGuid, 999);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ChangeInviteRoleAsync_WithValidId_UpdatesRole()
    {
        var cdpOrgGuid = Guid.NewGuid();
        var organisation = new CoreOrganisation
        {
            Id = 1,
            CdpOrganisationGuid = cdpOrgGuid,
            Name = "Test Org",
            Slug = "test-org"
        };

        var mapping = new InviteRoleMapping
        {
            Id = 1,
            CdpPersonInviteGuid = Guid.NewGuid(),
            OrganisationId = 1,
            OrganisationRole = OrganisationRole.Member,
            Organisation = organisation
        };

        _inviteRoleMappingRepositoryMock
            .Setup(r => r.GetByIdAsync(1, default))
            .ReturnsAsync(mapping);

        _organisationRepositoryMock
            .Setup(r => r.GetByCdpGuidAsync(cdpOrgGuid, default))
            .ReturnsAsync(organisation);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);

        await _service.ChangeInviteRoleAsync(cdpOrgGuid, 1, OrganisationRole.Admin);

        mapping.OrganisationRole.Should().Be(OrganisationRole.Admin);
        _inviteRoleMappingRepositoryMock.Verify(r => r.Update(mapping), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task AcceptInviteAsync_WithValidInvite_CreatesUserMembership()
    {
        var cdpOrgGuid = Guid.NewGuid();
        var userPrincipalId = "user-123";
        var organisation = new CoreOrganisation
        {
            Id = 1,
            CdpOrganisationGuid = cdpOrgGuid,
            Name = "Test Org",
            Slug = "test-org"
        };

        var mapping = new InviteRoleMapping
        {
            Id = 1,
            CdpPersonInviteGuid = Guid.NewGuid(),
            OrganisationId = 1,
            OrganisationRole = OrganisationRole.Admin,
            Organisation = organisation,
            ApplicationAssignments = new List<InviteRoleApplicationAssignment>
            {
                new()
                {
                    OrganisationApplicationId = 1,
                    ApplicationRoleId = 1
                }
            }
        };

        var request = new AcceptOrganisationInviteRequest
        {
            UserPrincipalId = userPrincipalId,
            CdpPersonId = Guid.NewGuid()
        };

        _inviteRoleMappingRepositoryMock
            .Setup(r => r.GetByIdAsync(1, default))
            .ReturnsAsync(mapping);

        _organisationRepositoryMock
            .Setup(r => r.GetByCdpGuidAsync(cdpOrgGuid, default))
            .ReturnsAsync(organisation);

        _membershipRepositoryMock
            .Setup(r => r.GetByUserAndOrganisationAsync(userPrincipalId, 1, default))
            .ReturnsAsync((UserOrganisationMembership?)null);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);

        await _service.AcceptInviteAsync(cdpOrgGuid, 1, request);

        _membershipRepositoryMock.Verify(
            r => r.Add(It.Is<UserOrganisationMembership>(m =>
                m.UserPrincipalId == userPrincipalId &&
                m.OrganisationId == 1 &&
                m.OrganisationRole == OrganisationRole.Admin)),
            Times.Once);

        _inviteRoleMappingRepositoryMock.Verify(r => r.Remove(mapping), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task AcceptInviteAsync_WhenUserAlreadyMember_ThrowsDuplicateEntityException()
    {
        var cdpOrgGuid = Guid.NewGuid();
        var userPrincipalId = "user-123";
        var organisation = new CoreOrganisation
        {
            Id = 1,
            CdpOrganisationGuid = cdpOrgGuid,
            Name = "Test Org",
            Slug = "test-org"
        };

        var mapping = new InviteRoleMapping
        {
            Id = 1,
            CdpPersonInviteGuid = Guid.NewGuid(),
            OrganisationId = 1,
            OrganisationRole = OrganisationRole.Admin,
            Organisation = organisation
        };

        var existingMembership = new UserOrganisationMembership
        {
            Id = 1,
            UserPrincipalId = userPrincipalId,
            OrganisationId = 1,
            OrganisationRole = OrganisationRole.Member
        };

        var request = new AcceptOrganisationInviteRequest
        {
            UserPrincipalId = userPrincipalId,
            CdpPersonId = Guid.NewGuid()
        };

        _inviteRoleMappingRepositoryMock
            .Setup(r => r.GetByIdAsync(1, default))
            .ReturnsAsync(mapping);

        _organisationRepositoryMock
            .Setup(r => r.GetByCdpGuidAsync(cdpOrgGuid, default))
            .ReturnsAsync(organisation);

        _membershipRepositoryMock
            .Setup(r => r.GetByUserAndOrganisationAsync(userPrincipalId, 1, default))
            .ReturnsAsync(existingMembership);

        var act = () => _service.AcceptInviteAsync(cdpOrgGuid, 1, request);

        await act.Should().ThrowAsync<DuplicateEntityException>();
    }
}
