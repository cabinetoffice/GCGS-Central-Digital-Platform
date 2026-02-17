using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Events;
using CO.CDP.UserManagement.Infrastructure.Subscribers;
using CO.CDP.UserManagement.Shared.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using UmOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Api.Tests.Subscribers;

public class PersonInviteClaimedSubscriberTests
{
    private readonly Mock<IInviteRoleMappingRepository> _inviteRoleMappingRepository;
    private readonly Mock<IUserOrganisationMembershipRepository> _membershipRepository;
    private readonly Mock<IUserApplicationAssignmentRepository> _assignmentRepository;
    private readonly Mock<ICdpMembershipSyncService> _membershipSyncService;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly PersonInviteClaimedSubscriber _subscriber;

    public PersonInviteClaimedSubscriberTests()
    {
        _inviteRoleMappingRepository = new Mock<IInviteRoleMappingRepository>();
        _membershipRepository = new Mock<IUserOrganisationMembershipRepository>();
        _assignmentRepository = new Mock<IUserApplicationAssignmentRepository>();
        _membershipSyncService = new Mock<ICdpMembershipSyncService>();
        _unitOfWork = new Mock<IUnitOfWork>();
        var logger = new Mock<ILogger<PersonInviteClaimedSubscriber>>();

        _membershipSyncService
            .Setup(s => s.SyncMembershipCreatedAsync(It.IsAny<UserOrganisationMembership>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _subscriber = new PersonInviteClaimedSubscriber(
            _inviteRoleMappingRepository.Object,
            _membershipRepository.Object,
            _assignmentRepository.Object,
            _membershipSyncService.Object,
            _unitOfWork.Object,
            logger.Object);
    }

    [Fact]
    public async Task Handle_WhenMappingExists_CreatesMembershipAndAssignments()
    {
        var orgGuid = Guid.NewGuid();
        var inviteGuid = Guid.NewGuid();
        var organisation = new UmOrganisation
        {
            Id = 10,
            CdpOrganisationGuid = orgGuid,
            Name = "Org",
            Slug = "org"
        };
        var applicationRole = new ApplicationRole
        {
            Id = 5,
            ApplicationId = 1,
            Name = "Role",
            IsActive = true
        };
        var organisationApplication = new OrganisationApplication
        {
            Id = 7,
            OrganisationId = organisation.Id,
            ApplicationId = 1,
            IsActive = true
        };
        var mapping = new InviteRoleMapping
        {
            Id = 1,
            CdpPersonInviteGuid = inviteGuid,
            OrganisationId = organisation.Id,
            Organisation = organisation,
            OrganisationRole = OrganisationRole.Admin,
            ApplicationAssignments = new List<InviteRoleApplicationAssignment>
            {
                new()
                {
                    OrganisationApplicationId = organisationApplication.Id,
                    ApplicationRoleId = applicationRole.Id,
                    OrganisationApplication = organisationApplication,
                    ApplicationRole = applicationRole
                }
            }
        };

        _inviteRoleMappingRepository
            .Setup(r => r.GetByCdpPersonInviteGuidAsync(inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mapping);

        _membershipRepository
            .Setup(r => r.GetByUserAndOrganisationAsync("user-urn", organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserOrganisationMembership?)null);

        var @event = new PersonInviteClaimed
        {
            PersonInviteGuid = inviteGuid,
            PersonGuid = Guid.NewGuid(),
            UserUrn = "user-urn",
            OrganisationGuid = orgGuid
        };

        await _subscriber.Handle(@event);

        _membershipRepository.Verify(r => r.Add(It.Is<UserOrganisationMembership>(m =>
            m.UserPrincipalId == "user-urn" &&
            m.OrganisationId == organisation.Id &&
            m.OrganisationRole == OrganisationRole.Admin)), Times.Once);

        _assignmentRepository.Verify(r => r.Add(It.Is<UserApplicationAssignment>(a =>
            a.OrganisationApplicationId == organisationApplication.Id &&
            a.Roles.Any(role => role.Id == applicationRole.Id))), Times.Once);

        _inviteRoleMappingRepository.Verify(r => r.Remove(mapping), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _membershipSyncService.Verify(s => s.SyncMembershipCreatedAsync(It.IsAny<UserOrganisationMembership>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMappingMissing_DoesNothing()
    {
        var inviteGuid = Guid.NewGuid();
        _inviteRoleMappingRepository
            .Setup(r => r.GetByCdpPersonInviteGuidAsync(inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InviteRoleMapping?)null);

        var @event = new PersonInviteClaimed
        {
            PersonInviteGuid = inviteGuid,
            PersonGuid = Guid.NewGuid(),
            UserUrn = "user-urn",
            OrganisationGuid = Guid.NewGuid()
        };

        await _subscriber.Handle(@event);

        _membershipRepository.Verify(r => r.Add(It.IsAny<UserOrganisationMembership>()), Times.Never);
        _assignmentRepository.Verify(r => r.Add(It.IsAny<UserApplicationAssignment>()), Times.Never);
        _inviteRoleMappingRepository.Verify(r => r.Remove(It.IsAny<InviteRoleMapping>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenMembershipExists_RemovesMappingOnly()
    {
        var orgGuid = Guid.NewGuid();
        var inviteGuid = Guid.NewGuid();
        var organisation = new UmOrganisation
        {
            Id = 10,
            CdpOrganisationGuid = orgGuid,
            Name = "Org",
            Slug = "org"
        };
        var mapping = new InviteRoleMapping
        {
            Id = 1,
            CdpPersonInviteGuid = inviteGuid,
            OrganisationId = organisation.Id,
            Organisation = organisation,
            OrganisationRole = OrganisationRole.Member
        };
        var membership = new UserOrganisationMembership
        {
            Id = 15,
            OrganisationId = organisation.Id,
            UserPrincipalId = "user-urn",
            OrganisationRole = OrganisationRole.Member
        };

        _inviteRoleMappingRepository
            .Setup(r => r.GetByCdpPersonInviteGuidAsync(inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mapping);

        _membershipRepository
            .Setup(r => r.GetByUserAndOrganisationAsync("user-urn", organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);

        var @event = new PersonInviteClaimed
        {
            PersonInviteGuid = inviteGuid,
            PersonGuid = Guid.NewGuid(),
            UserUrn = "user-urn",
            OrganisationGuid = orgGuid
        };

        await _subscriber.Handle(@event);

        _membershipRepository.Verify(r => r.Add(It.IsAny<UserOrganisationMembership>()), Times.Never);
        _assignmentRepository.Verify(r => r.Add(It.IsAny<UserApplicationAssignment>()), Times.Never);
        _inviteRoleMappingRepository.Verify(r => r.Remove(mapping), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _membershipSyncService.Verify(s => s.SyncMembershipCreatedAsync(It.IsAny<UserOrganisationMembership>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
