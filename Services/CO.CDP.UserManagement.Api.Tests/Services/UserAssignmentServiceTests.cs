using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Services;
using CO.CDP.UserManagement.Shared.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using CoreOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;
using CorePartyRole = CO.CDP.UserManagement.Core.Constants.PartyRole;

namespace CO.CDP.UserManagement.Api.Tests.Services;

public class UserAssignmentServiceTests
{
    private readonly Mock<IUserApplicationAssignmentRepository> _assignmentRepository = new();
    private readonly Mock<IUserOrganisationMembershipRepository> _membershipRepository = new();
    private readonly Mock<IOrganisationRepository> _organisationRepository = new();
    private readonly Mock<IOrganisationApplicationRepository> _organisationApplicationRepository = new();
    private readonly Mock<IRoleRepository> _roleRepository = new();
    private readonly Mock<IRoleMappingService> _roleMappingService = new();
    private readonly Mock<IOrganisationApiAdapter> _organisationApiAdapter = new();
    private readonly Mock<ICdpMembershipSyncService> _membershipSyncService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ILogger<UserAssignmentService>> _logger = new();

    private UserAssignmentService CreateSut() => new(
        _assignmentRepository.Object,
        _membershipRepository.Object,
        _organisationRepository.Object,
        _organisationApplicationRepository.Object,
        _roleRepository.Object,
        _roleMappingService.Object,
        _organisationApiAdapter.Object,
        _membershipSyncService.Object,
        _unitOfWork.Object,
        _logger.Object);

    [Fact]
    public async Task AssignDefaultApplicationsAsync_AddsBuyerFindATenderRoles()
    {
        var membership = new UserOrganisationMembership
        {
            Id = 73,
            OrganisationId = 42,
            OrganisationRoleId = (int)OrganisationRole.Owner
        };
        var organisation = new CoreOrganisation
        {
            Id = 42,
            CdpOrganisationGuid = Guid.NewGuid(),
            Name = "Acme",
            Slug = "acme"
        };
        var organisationApplication = new OrganisationApplication
        {
            Id = 17,
            OrganisationId = organisation.Id,
            ApplicationId = 1,
            IsActive = true,
            Application = new Application
            {
                Id = 1,
                ClientId = "find-a-tender",
                IsActive = true,
                IsDeleted = false,
                IsEnabledByDefault = true
            }
        };

        _roleMappingService
            .Setup(service => service.ShouldAutoAssignDefaultApplicationsAsync(membership, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _roleMappingService
            .Setup(service => service.GetInviteScopesAsync(membership.OrganisationRole, It.IsAny<CancellationToken>()))
            .ReturnsAsync(["ADMIN"]);
        _organisationApplicationRepository
            .Setup(repository => repository.GetDefaultEnabledByOrganisationIdAsync(organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([organisationApplication]);
        _assignmentRepository
            .Setup(repository => repository.GetByMembershipAndApplicationAsync(membership.Id, organisationApplication.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserApplicationAssignment?)null);
        _organisationRepository
            .Setup(repository => repository.GetByIdAsync(organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        _organisationApiAdapter
            .Setup(adapter => adapter.GetPartyRolesAsync(organisation.CdpOrganisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<CorePartyRole> { CorePartyRole.Buyer });
        _roleRepository
            .Setup(repository => repository.GetByApplicationIdAsync(organisationApplication.ApplicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new ApplicationRole
                {
                    Id = 5,
                    ApplicationId = organisationApplication.ApplicationId,
                    Name = "Viewer (buyer)",
                    IsActive = true,
                    SyncToOrganisationInformation = true,
                    RequiredPartyRoles = [CorePartyRole.Buyer],
                    OrganisationInformationScopes = ["VIEWER", "RESPONDER"]
                },
                new ApplicationRole
                {
                    Id = 6,
                    ApplicationId = organisationApplication.ApplicationId,
                    Name = "Editor (buyer)",
                    IsActive = true,
                    SyncToOrganisationInformation = true,
                    RequiredPartyRoles = [CorePartyRole.Buyer],
                    OrganisationInformationScopes = ["ADMIN", "RESPONDER"]
                },
                new ApplicationRole
                {
                    Id = 7,
                    ApplicationId = organisationApplication.ApplicationId,
                    Name = "Editor (supplier)",
                    IsActive = true,
                    SyncToOrganisationInformation = true,
                    RequiredPartyRoles = [CorePartyRole.Tenderer],
                    OrganisationInformationScopes = ["EDITOR", "RESPONDER"]
                }
            ]);

        await CreateSut().AssignDefaultApplicationsAsync(membership);

        _assignmentRepository.Verify(repository => repository.Add(It.Is<UserApplicationAssignment>(assignment =>
            assignment.UserOrganisationMembershipId == membership.Id &&
            assignment.OrganisationApplicationId == organisationApplication.Id &&
            assignment.Roles.Select(role => role.Id).OrderBy(id => id).SequenceEqual(new[] { 6 }))), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignDefaultApplicationsAsync_UpdatesActiveAssignment_WhenMatchingFindATenderRolesAreMissing()
    {
        var membership = new UserOrganisationMembership
        {
            Id = 73,
            OrganisationId = 42,
            OrganisationRoleId = (int)OrganisationRole.Owner
        };
        var organisation = new CoreOrganisation
        {
            Id = 42,
            CdpOrganisationGuid = Guid.NewGuid(),
            Name = "Acme",
            Slug = "acme"
        };
        var organisationApplication = new OrganisationApplication
        {
            Id = 17,
            OrganisationId = organisation.Id,
            ApplicationId = 1,
            IsActive = true,
            Application = new Application
            {
                Id = 1,
                ClientId = "find-a-tender",
                IsActive = true,
                IsDeleted = false,
                IsEnabledByDefault = true
            }
        };
        var existingAssignment = new UserApplicationAssignment
        {
            Id = 88,
            UserOrganisationMembershipId = membership.Id,
            OrganisationApplicationId = organisationApplication.Id,
            IsActive = true
        };

        _roleMappingService
            .Setup(service => service.ShouldAutoAssignDefaultApplicationsAsync(membership, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _roleMappingService
            .Setup(service => service.GetInviteScopesAsync(membership.OrganisationRole, It.IsAny<CancellationToken>()))
            .ReturnsAsync(["ADMIN"]);
        _organisationApplicationRepository
            .Setup(repository => repository.GetDefaultEnabledByOrganisationIdAsync(organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([organisationApplication]);
        _assignmentRepository
            .Setup(repository => repository.GetByMembershipAndApplicationAsync(membership.Id, organisationApplication.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAssignment);
        _organisationRepository
            .Setup(repository => repository.GetByIdAsync(organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        _organisationApiAdapter
            .Setup(adapter => adapter.GetPartyRolesAsync(organisation.CdpOrganisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<CorePartyRole> { CorePartyRole.Buyer, CorePartyRole.Tenderer });
        _roleRepository
            .Setup(repository => repository.GetByApplicationIdAsync(organisationApplication.ApplicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new ApplicationRole
                {
                    Id = 5,
                    ApplicationId = organisationApplication.ApplicationId,
                    Name = "Viewer (buyer)",
                    IsActive = true,
                    SyncToOrganisationInformation = true,
                    RequiredPartyRoles = [CorePartyRole.Buyer],
                    OrganisationInformationScopes = ["VIEWER", "RESPONDER"]
                },
                new ApplicationRole
                {
                    Id = 6,
                    ApplicationId = organisationApplication.ApplicationId,
                    Name = "Editor (buyer)",
                    IsActive = true,
                    SyncToOrganisationInformation = true,
                    RequiredPartyRoles = [CorePartyRole.Buyer],
                    OrganisationInformationScopes = ["ADMIN", "RESPONDER"]
                },
                new ApplicationRole
                {
                    Id = 7,
                    ApplicationId = organisationApplication.ApplicationId,
                    Name = "Editor (supplier)",
                    IsActive = true,
                    SyncToOrganisationInformation = true,
                    RequiredPartyRoles = [CorePartyRole.Tenderer],
                    OrganisationInformationScopes = ["EDITOR", "RESPONDER"]
                },
                new ApplicationRole
                {
                    Id = 8,
                    ApplicationId = organisationApplication.ApplicationId,
                    Name = "Viewer (supplier)",
                    IsActive = true,
                    SyncToOrganisationInformation = true,
                    RequiredPartyRoles = [CorePartyRole.Tenderer],
                    OrganisationInformationScopes = ["VIEWER", "RESPONDER"]
                }
            ]);

        await CreateSut().AssignDefaultApplicationsAsync(membership);

        _assignmentRepository.Verify(repository => repository.Update(It.Is<UserApplicationAssignment>(assignment =>
            assignment.Id == existingAssignment.Id &&
            assignment.Roles.Select(role => role.Id).OrderBy(id => id).SequenceEqual(new[] { 6, 7 }) &&
            assignment.ModifiedBy == "system:default-app-assignment")), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignDefaultApplicationsAsync_AddsViewerFindATenderRoles_ForMemberMembership()
    {
        var membership = new UserOrganisationMembership
        {
            Id = 73,
            OrganisationId = 42,
            OrganisationRoleId = (int)OrganisationRole.Member
        };
        var organisation = new CoreOrganisation
        {
            Id = 42,
            CdpOrganisationGuid = Guid.NewGuid(),
            Name = "Acme",
            Slug = "acme"
        };
        var organisationApplication = new OrganisationApplication
        {
            Id = 17,
            OrganisationId = organisation.Id,
            ApplicationId = 1,
            IsActive = true,
            Application = new Application
            {
                Id = 1,
                ClientId = "find-a-tender",
                IsActive = true,
                IsDeleted = false,
                IsEnabledByDefault = true
            }
        };

        _roleMappingService
            .Setup(service => service.ShouldAutoAssignDefaultApplicationsAsync(membership, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _roleMappingService
            .Setup(service => service.GetInviteScopesAsync(membership.OrganisationRole, It.IsAny<CancellationToken>()))
            .ReturnsAsync(["VIEWER"]);
        _organisationApplicationRepository
            .Setup(repository => repository.GetDefaultEnabledByOrganisationIdAsync(organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([organisationApplication]);
        _assignmentRepository
            .Setup(repository => repository.GetByMembershipAndApplicationAsync(membership.Id, organisationApplication.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserApplicationAssignment?)null);
        _organisationRepository
            .Setup(repository => repository.GetByIdAsync(organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        _organisationApiAdapter
            .Setup(adapter => adapter.GetPartyRolesAsync(organisation.CdpOrganisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<CorePartyRole> { CorePartyRole.Tenderer });
        _roleRepository
            .Setup(repository => repository.GetByApplicationIdAsync(organisationApplication.ApplicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new ApplicationRole
                {
                    Id = 7,
                    ApplicationId = organisationApplication.ApplicationId,
                    Name = "Editor (supplier)",
                    IsActive = true,
                    SyncToOrganisationInformation = true,
                    RequiredPartyRoles = [CorePartyRole.Tenderer],
                    OrganisationInformationScopes = ["EDITOR", "RESPONDER"]
                },
                new ApplicationRole
                {
                    Id = 8,
                    ApplicationId = organisationApplication.ApplicationId,
                    Name = "Viewer (supplier)",
                    IsActive = true,
                    SyncToOrganisationInformation = true,
                    RequiredPartyRoles = [CorePartyRole.Tenderer],
                    OrganisationInformationScopes = ["VIEWER", "RESPONDER"]
                }
            ]);

        await CreateSut().AssignDefaultApplicationsAsync(membership);

        _assignmentRepository.Verify(repository => repository.Add(It.Is<UserApplicationAssignment>(assignment =>
            assignment.UserOrganisationMembershipId == membership.Id &&
            assignment.OrganisationApplicationId == organisationApplication.Id &&
            assignment.Roles.Select(role => role.Id).OrderBy(id => id).SequenceEqual(new[] { 8 }))), Times.Once);
    }
}
