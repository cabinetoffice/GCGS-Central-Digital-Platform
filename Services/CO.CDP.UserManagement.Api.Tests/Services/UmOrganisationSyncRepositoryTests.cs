using System.Linq.Expressions;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Repositories;
using CO.CDP.UserManagement.Shared.Enums;
using FluentAssertions;
using Moq;
using Xunit.Sdk;
using CorePartyRole = CO.CDP.UserManagement.Core.Constants.PartyRole;
using UmOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Api.Tests.Services;

public class UmOrganisationSyncRepositoryTests
{
    private readonly Mock<IApplicationRepository> _applicationRepository = new();
    private readonly Mock<IUserOrganisationMembershipRepository> _membershipRepository = new();
    private readonly Mock<IOrganisationApiAdapter> _organisationApiAdapter = new();
    private readonly Mock<IOrganisationApplicationRepository> _organisationApplicationRepository = new();
    private readonly Mock<IOrganisationRepository> _organisationRepository = new();
    private readonly Mock<IRoleRepository> _roleRepository = new();
    private readonly Mock<ISlugGeneratorService> _slugGeneratorService = new();
    private readonly Mock<IUserApplicationAssignmentRepository> _userApplicationAssignmentRepository = new();

    public UmOrganisationSyncRepositoryTests()
    {
        _organisationApiAdapter
            .Setup(a => a.GetPartyRolesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISet<CorePartyRole>)new HashSet<CorePartyRole>());
    }

    private UmOrganisationSyncRepository CreateSut() => new(
        _organisationRepository.Object,
        _membershipRepository.Object,
        _applicationRepository.Object,
        _organisationApplicationRepository.Object,
        _userApplicationAssignmentRepository.Object,
        _roleRepository.Object,
        _slugGeneratorService.Object,
        _organisationApiAdapter.Object);

    [Fact]
    public async Task EnsureFounderOwnerCreatedAsync_AddsOwnerMembership_WhenFounderDoesNotExist()
    {
        var organisationGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        var organisation = new UmOrganisation
        {
            Id = 42,
            CdpOrganisationGuid = organisationGuid,
            Name = "Acme",
            Slug = "acme",
            IsActive = true,
            CreatedBy = "seed"
        };

        _organisationRepository
            .Setup(r => r.GetByCdpGuidAsync(organisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        _membershipRepository
            .Setup(r => r.GetByPersonIdAndOrganisationAsync(personGuid, organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserOrganisationMembership?)null);
        _membershipRepository
            .Setup(r => r.GetByUserAndOrganisationAsync("urn:example:user", organisation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserOrganisationMembership?)null);
        _organisationApplicationRepository
            .Setup(r => r.GetDefaultEnabledByOrganisationIdAsync(organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await CreateSut().EnsureFounderOwnerCreatedAsync(organisationGuid, personGuid, "urn:example:user");

        _membershipRepository.Verify(r => r.Add(It.Is<UserOrganisationMembership>(m =>
            m.UserPrincipalId == "urn:example:user" &&
            m.CdpPersonId == personGuid &&
            m.OrganisationId == organisation.Id &&
            m.OrganisationRoleId == (int)OrganisationRole.Owner &&
            m.IsActive &&
            m.CreatedBy == "system:org-sync")), Times.Once);
    }

    [Fact]
    public async Task EnsureFounderOwnerCreatedAsync_DoesNothing_WhenFounderMembershipAlreadyExistsByPerson()
    {
        var organisationGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        var organisation = new UmOrganisation
        {
            Id = 42,
            CdpOrganisationGuid = organisationGuid,
            Name = "Acme",
            Slug = "acme",
            IsActive = true,
            CreatedBy = "seed"
        };

        _organisationRepository
            .Setup(r => r.GetByCdpGuidAsync(organisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        _membershipRepository
            .Setup(r => r.GetByPersonIdAndOrganisationAsync(personGuid, organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserOrganisationMembership
                { Id = 7, OrganisationId = organisation.Id, UserPrincipalId = "urn:example:user" });
        _organisationApplicationRepository
            .Setup(r => r.GetDefaultEnabledByOrganisationIdAsync(organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await CreateSut().EnsureFounderOwnerCreatedAsync(organisationGuid, personGuid, "urn:example:user");

        _membershipRepository.Verify(r => r.Add(It.IsAny<UserOrganisationMembership>()), Times.Never);
    }

    [Fact]
    public async Task EnsureFounderOwnerCreatedAsync_DoesNothing_WhenFounderMembershipAlreadyExistsByUserPrincipal()
    {
        var organisationGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        var organisation = new UmOrganisation
        {
            Id = 42,
            CdpOrganisationGuid = organisationGuid,
            Name = "Acme",
            Slug = "acme",
            IsActive = true,
            CreatedBy = "seed"
        };

        _organisationRepository
            .Setup(r => r.GetByCdpGuidAsync(organisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        _membershipRepository
            .Setup(r => r.GetByPersonIdAndOrganisationAsync(personGuid, organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserOrganisationMembership?)null);
        _membershipRepository
            .Setup(r => r.GetByUserAndOrganisationAsync("urn:example:user", organisation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserOrganisationMembership
                { Id = 7, OrganisationId = organisation.Id, UserPrincipalId = "urn:example:user" });
        _organisationApplicationRepository
            .Setup(r => r.GetDefaultEnabledByOrganisationIdAsync(organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await CreateSut().EnsureFounderOwnerCreatedAsync(organisationGuid, personGuid, "urn:example:user");

        _membershipRepository.Verify(r => r.Add(It.IsAny<UserOrganisationMembership>()), Times.Never);
    }

    [Fact]
    public async Task EnsureFounderOwnerCreatedAsync_ReturnsFailure_WhenOrganisationHasNotBeenSynced()
    {
        var organisationGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();

        _organisationRepository
            .Setup(r => r.GetByCdpGuidAsync(organisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UmOrganisation?)null);

        var result = await CreateSut().EnsureFounderOwnerCreatedAsync(organisationGuid, personGuid, "urn:example:user");

        result.Match(
            onLeft: e =>
            {
                e.Should().Contain(organisationGuid.ToString());
                return true;
            },
            onRight: _ => throw new XunitException("Expected failure but got success"));
    }

    [Fact]
    public async Task EnsureActiveApplicationsEnabledAsync_AddsOnlyActiveNonDeletedApplications()
    {
        var organisationGuid = Guid.NewGuid();
        var organisation = new UmOrganisation
        {
            Id = 42,
            CdpOrganisationGuid = organisationGuid,
            Name = "Acme",
            Slug = "acme",
            IsActive = true,
            CreatedBy = "seed"
        };
        var applications = new List<Application>
        {
            new() { Id = 1, ClientId = "active-app", IsActive = true, IsDeleted = false },
            new() { Id = 2, ClientId = "inactive-app", IsActive = false, IsDeleted = false },
            new() { Id = 3, ClientId = "deleted-app", IsActive = true, IsDeleted = true }
        };

        _organisationRepository
            .Setup(r => r.GetByCdpGuidAsync(organisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        _applicationRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Application, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<Application, bool>> predicate, CancellationToken _) =>
                applications.Where(predicate.Compile()).ToList());
        _organisationApplicationRepository
            .Setup(r => r.GetByOrganisationAndApplicationAsync(organisation.Id, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganisationApplication?)null);

        await CreateSut().EnsureActiveApplicationsEnabledAsync(organisationGuid);

        _organisationApplicationRepository.Verify(r => r.Add(It.Is<OrganisationApplication>(oa =>
            oa.OrganisationId == organisation.Id &&
            oa.ApplicationId == 1 &&
            oa.IsActive &&
            !oa.IsDeleted &&
            oa.EnabledBy == "system:org-sync" &&
            oa.CreatedBy == "system:org-sync")), Times.Once);
        _organisationApplicationRepository.Verify(
            r => r.Add(It.Is<OrganisationApplication>(oa => oa.ApplicationId == 2)), Times.Never);
        _organisationApplicationRepository.Verify(
            r => r.Add(It.Is<OrganisationApplication>(oa => oa.ApplicationId == 3)), Times.Never);
    }

    [Fact]
    public async Task EnsureActiveApplicationsEnabledAsync_ReactivatesInactiveDeletedRelationships()
    {
        var organisationGuid = Guid.NewGuid();
        var organisation = new UmOrganisation
        {
            Id = 42,
            CdpOrganisationGuid = organisationGuid,
            Name = "Acme",
            Slug = "acme",
            IsActive = true,
            CreatedBy = "seed"
        };
        var application = new Application { Id = 1, ClientId = "find-a-tender", IsActive = true, IsDeleted = false };
        var existingRelationship = new OrganisationApplication
        {
            Id = 99,
            OrganisationId = organisation.Id,
            ApplicationId = application.Id,
            IsActive = false,
            IsDeleted = true,
            DisabledAt = DateTimeOffset.UtcNow,
            DeletedAt = DateTimeOffset.UtcNow,
            DeletedBy = "seed"
        };

        _organisationRepository
            .Setup(r => r.GetByCdpGuidAsync(organisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        _applicationRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Application, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<Application, bool>> predicate, CancellationToken _) =>
                new[] { application }.Where(predicate.Compile()).ToList());
        _organisationApplicationRepository
            .Setup(r => r.GetByOrganisationAndApplicationAsync(organisation.Id, application.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRelationship);

        await CreateSut().EnsureActiveApplicationsEnabledAsync(organisationGuid);

        _organisationApplicationRepository.Verify(r => r.Update(It.Is<OrganisationApplication>(oa =>
            oa.Id == existingRelationship.Id &&
            oa.IsActive &&
            !oa.IsDeleted &&
            oa.DisabledAt == null &&
            oa.DeletedAt == null &&
            oa.DeletedBy == null &&
            oa.EnabledBy == "system:org-sync" &&
            oa.ModifiedBy == "system:org-sync")), Times.Once);
    }

    [Fact]
    public async Task EnsureFounderOwnerCreatedAsync_AssignsBuyerFindATenderRoles_ForFounderMembership()
    {
        var organisationGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        var organisation = new UmOrganisation
        {
            Id = 42,
            CdpOrganisationGuid = organisationGuid,
            Name = "Acme",
            Slug = "acme",
            IsActive = true,
            CreatedBy = "seed"
        };
        var defaultOrganisationApplication = new OrganisationApplication
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
        UserOrganisationMembership? createdMembership = null;

        _organisationRepository
            .Setup(r => r.GetByCdpGuidAsync(organisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        _organisationApiAdapter
            .Setup(a => a.GetPartyRolesAsync(organisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISet<CorePartyRole>)new HashSet<CorePartyRole> { CorePartyRole.Buyer });
        _membershipRepository
            .Setup(r => r.GetByPersonIdAndOrganisationAsync(personGuid, organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserOrganisationMembership?)null);
        _membershipRepository
            .Setup(r => r.GetByUserAndOrganisationAsync("urn:example:user", organisation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserOrganisationMembership?)null);
        _membershipRepository
            .Setup(r => r.Add(It.IsAny<UserOrganisationMembership>()))
            .Callback<UserOrganisationMembership>(membership =>
            {
                membership.Id = 73;
                createdMembership = membership;
            });
        _organisationApplicationRepository
            .Setup(r => r.GetDefaultEnabledByOrganisationIdAsync(organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([defaultOrganisationApplication]);
        _roleRepository
            .Setup(r => r.GetByApplicationIdAsync(defaultOrganisationApplication.ApplicationId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new ApplicationRole
                {
                    Id = 5,
                    ApplicationId = defaultOrganisationApplication.ApplicationId,
                    Name = "Viewer (buyer)",
                    IsActive = true,
                    SyncToOrganisationInformation = true,
                    RequiredPartyRoles = [CorePartyRole.Buyer],
                    OrganisationInformationScopes = ["VIEWER", "RESPONDER"]
                },
                new ApplicationRole
                {
                    Id = 6,
                    ApplicationId = defaultOrganisationApplication.ApplicationId,
                    Name = "Editor (buyer)",
                    IsActive = true,
                    SyncToOrganisationInformation = true,
                    RequiredPartyRoles = [CorePartyRole.Buyer],
                    OrganisationInformationScopes = ["ADMIN", "RESPONDER"]
                },
                new ApplicationRole
                {
                    Id = 7,
                    ApplicationId = defaultOrganisationApplication.ApplicationId,
                    Name = "Editor (supplier)",
                    IsActive = true,
                    SyncToOrganisationInformation = true,
                    RequiredPartyRoles = [CorePartyRole.Tenderer],
                    OrganisationInformationScopes = ["EDITOR", "RESPONDER"]
                }
            ]);
        _userApplicationAssignmentRepository
            .Setup(r => r.GetByMembershipAndApplicationAsync(73, defaultOrganisationApplication.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserApplicationAssignment?)null);

        await CreateSut().EnsureFounderOwnerCreatedAsync(
            organisationGuid,
            personGuid,
            "urn:example:user");

        createdMembership.Should().NotBeNull();
        _userApplicationAssignmentRepository.Verify(r => r.Add(It.Is<UserApplicationAssignment>(assignment =>
            assignment.UserOrganisationMembership == createdMembership &&
            assignment.OrganisationApplicationId == defaultOrganisationApplication.Id &&
            assignment.IsActive &&
            !assignment.IsDeleted &&
            assignment.AssignedBy == "system:org-sync" &&
            assignment.CreatedBy == "system:org-sync" &&
            assignment.Roles.Select(role => role.Id).OrderBy(id => id).SequenceEqual(new[] { 6 }))), Times.Once);
    }

    [Fact]
    public async Task EnsureFounderOwnerCreatedAsync_ReactivatesExistingDefaultApplicationAssignment()
    {
        var organisationGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        var organisation = new UmOrganisation
        {
            Id = 42,
            CdpOrganisationGuid = organisationGuid,
            Name = "Acme",
            Slug = "acme",
            IsActive = true,
            CreatedBy = "seed"
        };
        var existingMembership = new UserOrganisationMembership
        {
            Id = 73,
            OrganisationId = organisation.Id,
            UserPrincipalId = "urn:example:user"
        };
        var defaultOrganisationApplication = new OrganisationApplication
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
            UserOrganisationMembershipId = existingMembership.Id,
            OrganisationApplicationId = defaultOrganisationApplication.Id,
            IsActive = false,
            IsDeleted = true,
            RevokedAt = DateTimeOffset.UtcNow,
            DeletedAt = DateTimeOffset.UtcNow,
            DeletedBy = "seed"
        };

        _organisationRepository
            .Setup(r => r.GetByCdpGuidAsync(organisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        _organisationApiAdapter
            .Setup(a => a.GetPartyRolesAsync(organisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISet<CorePartyRole>)new HashSet<CorePartyRole> { CorePartyRole.Tenderer });
        _membershipRepository
            .Setup(r => r.GetByPersonIdAndOrganisationAsync(personGuid, organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMembership);
        _organisationApplicationRepository
            .Setup(r => r.GetDefaultEnabledByOrganisationIdAsync(organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([defaultOrganisationApplication]);
        _userApplicationAssignmentRepository
            .Setup(r => r.GetByMembershipAndApplicationAsync(existingMembership.Id, defaultOrganisationApplication.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAssignment);

        _roleRepository
            .Setup(r => r.GetByApplicationIdAsync(defaultOrganisationApplication.ApplicationId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new ApplicationRole
                {
                    Id = 7,
                    ApplicationId = defaultOrganisationApplication.ApplicationId,
                    Name = "Editor (supplier)",
                    IsActive = true,
                    SyncToOrganisationInformation = true,
                    RequiredPartyRoles = [CorePartyRole.Tenderer],
                    OrganisationInformationScopes = ["EDITOR", "RESPONDER"]
                }
            ]);

        await CreateSut().EnsureFounderOwnerCreatedAsync(
            organisationGuid,
            personGuid,
            "urn:example:user");

        _membershipRepository.Verify(r => r.Add(It.IsAny<UserOrganisationMembership>()), Times.Never);
        _userApplicationAssignmentRepository.Verify(r => r.Update(It.Is<UserApplicationAssignment>(assignment =>
            assignment.Id == existingAssignment.Id &&
            assignment.IsActive &&
            !assignment.IsDeleted &&
            assignment.RevokedAt == null &&
            assignment.DeletedAt == null &&
            assignment.DeletedBy == null &&
            assignment.AssignedBy == "system:org-sync" &&
            assignment.ModifiedBy == "system:org-sync" &&
            assignment.Roles.Select(role => role.Id).OrderBy(id => id).SequenceEqual(new[] { 7 }))), Times.Once);
    }

    [Fact]
    public async Task
        EnsureFounderOwnerCreatedAsync_AssignsBuyerAndSupplierFindATenderRoles_WhenOrganisationHasBothPartyRoles()
    {
        var organisationGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        var organisation = new UmOrganisation
        {
            Id = 42,
            CdpOrganisationGuid = organisationGuid,
            Name = "Acme",
            Slug = "acme",
            IsActive = true,
            CreatedBy = "seed"
        };
        var defaultOrganisationApplication = new OrganisationApplication
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

        _organisationRepository
            .Setup(r => r.GetByCdpGuidAsync(organisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        _organisationApiAdapter
            .Setup(a => a.GetPartyRolesAsync(organisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISet<CorePartyRole>)new HashSet<CorePartyRole>
                { CorePartyRole.Buyer, CorePartyRole.Tenderer });
        _membershipRepository
            .Setup(r => r.GetByPersonIdAndOrganisationAsync(personGuid, organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserOrganisationMembership
            {
                Id = 73,
                OrganisationId = organisation.Id,
                UserPrincipalId = "urn:example:user"
            });
        _organisationApplicationRepository
            .Setup(r => r.GetDefaultEnabledByOrganisationIdAsync(organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([defaultOrganisationApplication]);
        _roleRepository
            .Setup(r => r.GetByApplicationIdAsync(defaultOrganisationApplication.ApplicationId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new ApplicationRole
                {
                    Id = 5,
                    ApplicationId = defaultOrganisationApplication.ApplicationId,
                    Name = "Viewer (buyer)",
                    IsActive = true,
                    SyncToOrganisationInformation = true,
                    RequiredPartyRoles = [CorePartyRole.Buyer],
                    OrganisationInformationScopes = ["VIEWER", "RESPONDER"]
                },
                new ApplicationRole
                {
                    Id = 6,
                    ApplicationId = defaultOrganisationApplication.ApplicationId,
                    Name = "Editor (buyer)",
                    IsActive = true,
                    SyncToOrganisationInformation = true,
                    RequiredPartyRoles = [CorePartyRole.Buyer],
                    OrganisationInformationScopes = ["ADMIN", "RESPONDER"]
                },
                new ApplicationRole
                {
                    Id = 7,
                    ApplicationId = defaultOrganisationApplication.ApplicationId,
                    Name = "Editor (supplier)",
                    IsActive = true,
                    SyncToOrganisationInformation = true,
                    RequiredPartyRoles = [CorePartyRole.Tenderer],
                    OrganisationInformationScopes = ["EDITOR", "RESPONDER"]
                }
            ]);
        _userApplicationAssignmentRepository
            .Setup(r => r.GetByMembershipAndApplicationAsync(73, defaultOrganisationApplication.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserApplicationAssignment?)null);

        await CreateSut().EnsureFounderOwnerCreatedAsync(
            organisationGuid,
            personGuid,
            "urn:example:user");

        _userApplicationAssignmentRepository.Verify(r => r.Add(It.Is<UserApplicationAssignment>(assignment =>
            assignment.Roles.Select(role => role.Id).OrderBy(id => id).SequenceEqual(new[] { 6, 7 }))), Times.Once);
    }

    [Fact]
    public async Task EnsureFounderOwnerCreatedAsync_DoesNotUpdateAssignment_WhenExpectedFindATenderRolesAlreadyExist()
    {
        var organisationGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        var organisation = new UmOrganisation
        {
            Id = 42,
            CdpOrganisationGuid = organisationGuid,
            Name = "Acme",
            Slug = "acme",
            IsActive = true,
            CreatedBy = "seed"
        };
        var existingMembership = new UserOrganisationMembership
        {
            Id = 73,
            OrganisationId = organisation.Id,
            UserPrincipalId = "urn:example:user"
        };
        var existingRole = new ApplicationRole
        {
            Id = 7,
            ApplicationId = 1,
            Name = "Editor (supplier)",
            IsActive = true,
            SyncToOrganisationInformation = true,
            RequiredPartyRoles = [CorePartyRole.Tenderer],
            OrganisationInformationScopes = ["EDITOR", "RESPONDER"]
        };
        var defaultOrganisationApplication = new OrganisationApplication
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
            UserOrganisationMembershipId = existingMembership.Id,
            OrganisationApplicationId = defaultOrganisationApplication.Id,
            IsActive = true,
            Roles = [existingRole]
        };

        _organisationRepository
            .Setup(r => r.GetByCdpGuidAsync(organisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        _organisationApiAdapter
            .Setup(a => a.GetPartyRolesAsync(organisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISet<CorePartyRole>)new HashSet<CorePartyRole> { CorePartyRole.Tenderer });
        _membershipRepository
            .Setup(r => r.GetByPersonIdAndOrganisationAsync(personGuid, organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMembership);
        _organisationApplicationRepository
            .Setup(r => r.GetDefaultEnabledByOrganisationIdAsync(organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([defaultOrganisationApplication]);
        _roleRepository
            .Setup(r => r.GetByApplicationIdAsync(defaultOrganisationApplication.ApplicationId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([existingRole]);
        _userApplicationAssignmentRepository
            .Setup(r => r.GetByMembershipAndApplicationAsync(existingMembership.Id, defaultOrganisationApplication.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAssignment);

        await CreateSut().EnsureFounderOwnerCreatedAsync(
            organisationGuid,
            personGuid,
            "urn:example:user");

        _userApplicationAssignmentRepository.Verify(r => r.Update(It.IsAny<UserApplicationAssignment>()), Times.Never);
    }

    [Fact]
    public async Task
        EnsureMemberScopesAndAppRolesUpdatedAsync_UpdatesRoleAndRecalculatesAppRoles_WhenMembershipExists()
    {
        var organisationGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        var organisation = new UmOrganisation
        {
            Id = 42,
            CdpOrganisationGuid = organisationGuid,
            Name = "Acme",
            Slug = "acme",
            IsActive = true,
            CreatedBy = "seed"
        };
        var membership = new UserOrganisationMembership
        {
            Id = 73,
            OrganisationId = organisation.Id,
            CdpPersonId = personGuid,
            UserPrincipalId = "urn:example:user",
            OrganisationRoleId = (int)OrganisationRole.Member
        };
        var defaultOrganisationApplication = new OrganisationApplication
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

        _organisationRepository
            .Setup(r => r.GetByCdpGuidAsync(organisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        _organisationApiAdapter
            .Setup(a => a.GetPartyRolesAsync(organisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISet<CorePartyRole>)new HashSet<CorePartyRole> { CorePartyRole.Buyer });
        _membershipRepository
            .Setup(r => r.GetByPersonIdAndOrganisationAsync(personGuid, organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);
        _organisationApplicationRepository
            .Setup(r => r.GetDefaultEnabledByOrganisationIdAsync(organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([defaultOrganisationApplication]);
        _roleRepository
            .Setup(r => r.GetByApplicationIdAsync(defaultOrganisationApplication.ApplicationId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new ApplicationRole
                {
                    Id = 6,
                    ApplicationId = defaultOrganisationApplication.ApplicationId,
                    Name = "Editor (buyer)",
                    IsActive = true,
                    SyncToOrganisationInformation = true,
                    RequiredPartyRoles = [CorePartyRole.Buyer],
                    OrganisationInformationScopes = ["ADMIN", "RESPONDER"]
                }
            ]);
        _userApplicationAssignmentRepository
            .Setup(r => r.GetByMembershipAndApplicationAsync(membership.Id, defaultOrganisationApplication.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserApplicationAssignment?)null);

        var result = await CreateSut().EnsureMemberScopesAndAppRolesUpdatedAsync(
            organisationGuid, personGuid, ["ADMIN"]);

        result.IsSuccess.Should().BeTrue();
        _membershipRepository.Verify(r => r.Update(It.Is<UserOrganisationMembership>(m =>
            m.OrganisationRoleId == (int)OrganisationRole.Admin &&
            m.ModifiedBy == "system:org-sync")), Times.Once);
        _userApplicationAssignmentRepository.Verify(r => r.Add(It.Is<UserApplicationAssignment>(a =>
            a.UserOrganisationMembership == membership &&
            a.OrganisationApplicationId == defaultOrganisationApplication.Id &&
            a.Roles.Select(role => role.Id).OrderBy(id => id).SequenceEqual(new[] { 6 }))), Times.Once);
    }

    [Fact]
    public async Task EnsureMemberScopesAndAppRolesUpdatedAsync_ReturnsSuccess_WhenOrgNotFound()
    {
        var orgGuid = Guid.NewGuid();
        _organisationRepository.Setup(r => r.GetByCdpGuidAsync(orgGuid, default))
            .ReturnsAsync((UmOrganisation?)null);

        var result = await CreateSut().EnsureMemberScopesAndAppRolesUpdatedAsync(
            orgGuid, Guid.NewGuid(), ["ADMIN"]);

        result.IsSuccess.Should().BeTrue();
        _membershipRepository.Verify(r => r.GetByPersonIdAndOrganisationAsync(
            It.IsAny<Guid>(), It.IsAny<int>(), default), Times.Never);
    }

    [Fact]
    public async Task EnsureMemberScopesAndAppRolesUpdatedAsync_ReturnsSuccess_WhenMembershipNotFound()
    {
        var orgGuid = Guid.NewGuid();
        var org = new UmOrganisation
        {
            Id = 1, CdpOrganisationGuid = orgGuid, Name = "Test", Slug = "test", IsActive = true, CreatedBy = "seed"
        };
        _organisationRepository.Setup(r => r.GetByCdpGuidAsync(orgGuid, default)).ReturnsAsync(org);
        _membershipRepository.Setup(r => r.GetByPersonIdAndOrganisationAsync(
                It.IsAny<Guid>(), org.Id, default))
            .ReturnsAsync((UserOrganisationMembership?)null);

        var result = await CreateSut().EnsureMemberScopesAndAppRolesUpdatedAsync(
            orgGuid, Guid.NewGuid(), ["ADMIN"]);

        result.IsSuccess.Should().BeTrue();
        _membershipRepository.Verify(r => r.Update(It.IsAny<UserOrganisationMembership>()), Times.Never);
    }
}