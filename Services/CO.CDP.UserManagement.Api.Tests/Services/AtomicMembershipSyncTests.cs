using CO.CDP.OrganisationSync;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using CoreOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;
using CorePartyRole = CO.CDP.UserManagement.Core.Constants.PartyRole;

namespace CO.CDP.UserManagement.Api.Tests.Services;

public class AtomicMembershipSyncTests
{
    private readonly Mock<IAtomicScope> _atomicScope = new();
    private readonly Mock<IOrganisationRepository> _organisationRepository = new();
    private readonly Mock<IUserOrganisationMembershipRepository> _membershipRepository = new();
    private readonly Mock<IUserApplicationAssignmentRepository> _assignmentRepository = new();
    private readonly Mock<IOrganisationApplicationRepository> _organisationApplicationRepository = new();
    private readonly Mock<IRoleRepository> _roleRepository = new();
    private readonly Mock<IInviteRoleMappingRepository> _inviteRoleMappingRepository = new();
    private readonly Mock<IRoleMappingService> _roleMappingService = new();
    private readonly Mock<IOrganisationPersonSyncRepository> _organisationPersonSyncRepository = new();
    private readonly Mock<IOrganisationApiAdapter> _organisationApiAdapter = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    public AtomicMembershipSyncTests()
    {
        _atomicScope
            .Setup(s => s.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<CO.CDP.Functional.Unit>>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<CO.CDP.Functional.Unit>>, CancellationToken>((action, ct) => action(ct));

        _atomicScope
            .Setup(s => s.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<UserOrganisationMembership>>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<UserOrganisationMembership>>, CancellationToken>((action, ct) => action(ct));

        _atomicScope
            .Setup(s => s.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<UserApplicationAssignment>>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<UserApplicationAssignment>>, CancellationToken>((action, ct) => action(ct));

        _currentUserService.Setup(s => s.GetUserPrincipalId()).Returns("urn:fdc:gov.uk:2022:test-actor");
    }

    private AtomicMembershipSync CreateSut() => new(
        _atomicScope.Object,
        _organisationRepository.Object,
        _membershipRepository.Object,
        _assignmentRepository.Object,
        _organisationApplicationRepository.Object,
        _roleRepository.Object,
        _inviteRoleMappingRepository.Object,
        _roleMappingService.Object,
        _organisationPersonSyncRepository.Object,
        _organisationApiAdapter.Object,
        _currentUserService.Object,
        _unitOfWork.Object,
        NullLogger<AtomicMembershipSync>.Instance);

    private static CoreOrganisation GivenOrganisation(Guid cdpGuid) => new()
    {
        Id = 42,
        CdpOrganisationGuid = cdpGuid,
        Name = "Acme",
        Slug = "acme",
        IsActive = true,
        CreatedBy = "seed"
    };

    private static UserOrganisationMembership GivenMembership(
        Guid personId, int orgId, bool isActive = true, OrganisationRole role = OrganisationRole.Member) =>
        new()
        {
            Id = 10,
            CdpPersonId = personId,
            OrganisationId = orgId,
            UserPrincipalId = "urn:fdc:gov.uk:2022:test-user",
            OrganisationRoleId = (int)role,
            IsActive = isActive,
            JoinedAt = DateTimeOffset.UtcNow,
            CreatedBy = "seed"
        };

    // ── Remove: happy path ──────────────────────────────────────────────────

    [Fact]
    public async Task Remove_HappyPath_SoftDeletesMembership()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var org = GivenOrganisation(orgGuid);
        var membership = GivenMembership(personId, org.Id);

        _organisationRepository.Setup(r => r.GetByCdpGuidAsync(orgGuid, It.IsAny<CancellationToken>())).ReturnsAsync(org);
        _membershipRepository.Setup(r => r.GetByPersonIdAndOrganisationAsync(personId, org.Id, It.IsAny<CancellationToken>())).ReturnsAsync(membership);
        _assignmentRepository.Setup(r => r.GetByMembershipIdAsync(membership.Id, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        await CreateSut().RemoveUserFromOrganisationAsync(orgGuid, personId);

        membership.IsActive.Should().BeFalse();
        membership.IsDeleted.Should().BeTrue();
        membership.DeletedBy.Should().Be("urn:fdc:gov.uk:2022:test-actor");
        membership.DeletedAt.Should().NotBeNull();
        _membershipRepository.Verify(r => r.Update(membership), Times.Once);
    }

    [Fact]
    public async Task Remove_HappyPath_RevokesActiveAssignments()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var org = GivenOrganisation(orgGuid);
        var membership = GivenMembership(personId, org.Id);
        var assignment = new UserApplicationAssignment { Id = 99, UserOrganisationMembershipId = membership.Id, IsActive = true };

        _organisationRepository.Setup(r => r.GetByCdpGuidAsync(orgGuid, It.IsAny<CancellationToken>())).ReturnsAsync(org);
        _membershipRepository.Setup(r => r.GetByPersonIdAndOrganisationAsync(personId, org.Id, It.IsAny<CancellationToken>())).ReturnsAsync(membership);
        _assignmentRepository.Setup(r => r.GetByMembershipIdAsync(membership.Id, It.IsAny<CancellationToken>())).ReturnsAsync([assignment]);

        await CreateSut().RemoveUserFromOrganisationAsync(orgGuid, personId);

        assignment.IsActive.Should().BeFalse();
        assignment.RevokedBy.Should().Be("urn:fdc:gov.uk:2022:test-actor");
        assignment.RevokedAt.Should().NotBeNull();
        _assignmentRepository.Verify(r => r.Update(assignment), Times.Once);
    }

    [Fact]
    public async Task Remove_HappyPath_RemovesOiScopes()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var org = GivenOrganisation(orgGuid);
        var membership = GivenMembership(personId, org.Id);

        _organisationRepository.Setup(r => r.GetByCdpGuidAsync(orgGuid, It.IsAny<CancellationToken>())).ReturnsAsync(org);
        _membershipRepository.Setup(r => r.GetByPersonIdAndOrganisationAsync(personId, org.Id, It.IsAny<CancellationToken>())).ReturnsAsync(membership);
        _assignmentRepository.Setup(r => r.GetByMembershipIdAsync(membership.Id, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        await CreateSut().RemoveUserFromOrganisationAsync(orgGuid, personId);

        _organisationPersonSyncRepository.Verify(
            r => r.RemoveAsync(org.CdpOrganisationGuid, personId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Remove_WhenOrganisationNotFound_ThrowsEntityNotFoundException()
    {
        var orgGuid = Guid.NewGuid();
        _organisationRepository.Setup(r => r.GetByCdpGuidAsync(orgGuid, It.IsAny<CancellationToken>())).ReturnsAsync((CoreOrganisation?)null);

        var act = () => CreateSut().RemoveUserFromOrganisationAsync(orgGuid, Guid.NewGuid());

        await act.Should().ThrowAsync<EntityNotFoundException>().WithMessage("*Organisation*");
    }

    [Fact]
    public async Task Remove_WhenMembershipNotFound_ThrowsEntityNotFoundException()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var org = GivenOrganisation(orgGuid);

        _organisationRepository.Setup(r => r.GetByCdpGuidAsync(orgGuid, It.IsAny<CancellationToken>())).ReturnsAsync(org);
        _membershipRepository.Setup(r => r.GetByPersonIdAndOrganisationAsync(personId, org.Id, It.IsAny<CancellationToken>())).ReturnsAsync((UserOrganisationMembership?)null);

        var act = () => CreateSut().RemoveUserFromOrganisationAsync(orgGuid, personId);

        await act.Should().ThrowAsync<EntityNotFoundException>().WithMessage("*UserOrganisationMembership*");
    }

    [Fact]
    public async Task Remove_WhenAlreadyInactive_IsIdempotentAndMakesNoWrites()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var org = GivenOrganisation(orgGuid);
        var membership = GivenMembership(personId, org.Id, isActive: false);

        _organisationRepository.Setup(r => r.GetByCdpGuidAsync(orgGuid, It.IsAny<CancellationToken>())).ReturnsAsync(org);
        _membershipRepository.Setup(r => r.GetByPersonIdAndOrganisationAsync(personId, org.Id, It.IsAny<CancellationToken>())).ReturnsAsync(membership);

        await CreateSut().RemoveUserFromOrganisationAsync(orgGuid, personId);

        _membershipRepository.Verify(r => r.Update(It.IsAny<UserOrganisationMembership>()), Times.Never);
        _organisationPersonSyncRepository.Verify(r => r.RemoveAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Remove_WhenLastOwner_ThrowsLastOwnerRemovalException()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var org = GivenOrganisation(orgGuid);
        var membership = GivenMembership(personId, org.Id, role: OrganisationRole.Owner);

        _organisationRepository.Setup(r => r.GetByCdpGuidAsync(orgGuid, It.IsAny<CancellationToken>())).ReturnsAsync(org);
        _membershipRepository.Setup(r => r.GetByPersonIdAndOrganisationAsync(personId, org.Id, It.IsAny<CancellationToken>())).ReturnsAsync(membership);
        _membershipRepository.Setup(r => r.CountActiveOwnersByOrganisationIdAsync(org.Id, It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var act = () => CreateSut().RemoveUserFromOrganisationAsync(orgGuid, personId);

        await act.Should().ThrowAsync<LastOwnerRemovalException>().WithMessage("*last Owner*");
    }

    [Fact]
    public async Task Remove_WhenOwnerButNotLast_Succeeds()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var org = GivenOrganisation(orgGuid);
        var membership = GivenMembership(personId, org.Id, role: OrganisationRole.Owner);

        _organisationRepository.Setup(r => r.GetByCdpGuidAsync(orgGuid, It.IsAny<CancellationToken>())).ReturnsAsync(org);
        _membershipRepository.Setup(r => r.GetByPersonIdAndOrganisationAsync(personId, org.Id, It.IsAny<CancellationToken>())).ReturnsAsync(membership);
        _membershipRepository.Setup(r => r.CountActiveOwnersByOrganisationIdAsync(org.Id, It.IsAny<CancellationToken>())).ReturnsAsync(2);
        _assignmentRepository.Setup(r => r.GetByMembershipIdAsync(membership.Id, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        await CreateSut().RemoveUserFromOrganisationAsync(orgGuid, personId);

        _membershipRepository.Verify(r => r.Update(membership), Times.Once);
    }

    // ── UpdateMembershipRole ────────────────────────────────────────────────

    [Fact]
    public async Task UpdateRole_HappyPath_AppliesRoleAndUpsertsScopesAtomically()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var org = GivenOrganisation(orgGuid);
        var membership = GivenMembership(personId, org.Id);

        _organisationRepository.Setup(r => r.GetByCdpGuidAsync(orgGuid, It.IsAny<CancellationToken>())).ReturnsAsync(org);
        _membershipRepository.Setup(r => r.GetByPersonIdAndOrganisationAsync(personId, org.Id, It.IsAny<CancellationToken>())).ReturnsAsync(membership);
        _roleMappingService.Setup(r => r.ApplyRoleDefinitionAsync(membership, OrganisationRole.Admin, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await CreateSut().UpdateMembershipRoleAsync(orgGuid, personId, OrganisationRole.Admin);

        result.Should().Be(membership);
        _membershipRepository.Verify(r => r.Update(membership), Times.Once);
        _organisationPersonSyncRepository.Verify(
            r => r.UpsertAsync(org.CdpOrganisationGuid, personId, It.Is<IReadOnlyList<string>>(s => s.Contains("ADMIN")), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateRole_WhenSyncDisabled_DoesNotUpsertOiScopes()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var org = GivenOrganisation(orgGuid);
        var membership = GivenMembership(personId, org.Id);

        _organisationRepository.Setup(r => r.GetByCdpGuidAsync(orgGuid, It.IsAny<CancellationToken>())).ReturnsAsync(org);
        _membershipRepository.Setup(r => r.GetByPersonIdAndOrganisationAsync(personId, org.Id, It.IsAny<CancellationToken>())).ReturnsAsync(membership);
        _roleMappingService.Setup(r => r.ApplyRoleDefinitionAsync(membership, OrganisationRole.Member, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await CreateSut().UpdateMembershipRoleAsync(orgGuid, personId, OrganisationRole.Member);

        _organisationPersonSyncRepository.Verify(
            r => r.UpsertAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateRole_WhenOrganisationNotFound_ThrowsEntityNotFoundException()
    {
        var orgGuid = Guid.NewGuid();
        _organisationRepository.Setup(r => r.GetByCdpGuidAsync(orgGuid, It.IsAny<CancellationToken>())).ReturnsAsync((CoreOrganisation?)null);

        var act = () => CreateSut().UpdateMembershipRoleAsync(orgGuid, Guid.NewGuid(), OrganisationRole.Admin);

        await act.Should().ThrowAsync<EntityNotFoundException>().WithMessage("*Organisation*");
    }

    [Fact]
    public async Task UpdateRole_WhenMembershipNotFound_ThrowsEntityNotFoundException()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var org = GivenOrganisation(orgGuid);

        _organisationRepository.Setup(r => r.GetByCdpGuidAsync(orgGuid, It.IsAny<CancellationToken>())).ReturnsAsync(org);
        _membershipRepository.Setup(r => r.GetByPersonIdAndOrganisationAsync(personId, org.Id, It.IsAny<CancellationToken>())).ReturnsAsync((UserOrganisationMembership?)null);

        var act = () => CreateSut().UpdateMembershipRoleAsync(orgGuid, personId, OrganisationRole.Admin);

        await act.Should().ThrowAsync<EntityNotFoundException>().WithMessage("*UserOrganisationMembership*");
    }

    [Fact]
    public async Task UpdateRole_WrapsInAtomicScope()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var org = GivenOrganisation(orgGuid);
        var membership = GivenMembership(personId, org.Id);

        _organisationRepository.Setup(r => r.GetByCdpGuidAsync(orgGuid, It.IsAny<CancellationToken>())).ReturnsAsync(org);
        _membershipRepository.Setup(r => r.GetByPersonIdAndOrganisationAsync(personId, org.Id, It.IsAny<CancellationToken>())).ReturnsAsync(membership);
        _roleMappingService.Setup(r => r.ApplyRoleDefinitionAsync(membership, OrganisationRole.Admin, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await CreateSut().UpdateMembershipRoleAsync(orgGuid, personId, OrganisationRole.Admin);

        _atomicScope.Verify(
            s => s.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<UserOrganisationMembership>>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── AssignUserToApplication ──────────────────────────────────────────────

    [Fact]
    public async Task AssignUser_HappyPath_CreatesAssignmentAndSyncsOi()
    {
        var membership = GivenMembership(Guid.NewGuid(), 42);
        var orgApp = new OrganisationApplication { Id = 5, OrganisationId = 42, ApplicationId = 1, IsActive = true };
        var role = new ApplicationRole { Id = 1, ApplicationId = 1, IsActive = true };

        _membershipRepository.Setup(r => r.GetByPersonIdAndOrganisationAsync(membership.CdpPersonId!.Value, 42, It.IsAny<CancellationToken>())).ReturnsAsync(membership);
        _organisationApplicationRepository.Setup(r => r.GetByOrganisationAndApplicationAsync(42, 1, It.IsAny<CancellationToken>())).ReturnsAsync(orgApp);
        _assignmentRepository.Setup(r => r.GetByMembershipAndApplicationAsync(membership.Id, orgApp.Id, It.IsAny<CancellationToken>())).ReturnsAsync((UserApplicationAssignment?)null);
        _roleMappingService.Setup(r => r.GetAssignableRolesAsync(42, membership.OrganisationRole, It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync([role]);
        _roleMappingService.Setup(r => r.GetOrganisationInformationScopesAsync(membership.Id, It.IsAny<CancellationToken>())).ReturnsAsync(["ADMIN"]);
        _organisationRepository.Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>())).ReturnsAsync(new CoreOrganisation { Id = 42, CdpOrganisationGuid = Guid.NewGuid(), Name = "Acme", Slug = "acme", IsActive = true, CreatedBy = "seed" });

        var result = await CreateSut().AssignUserToApplicationAsync(membership.CdpPersonId!.Value.ToString(), 42, 1, [1]);

        _assignmentRepository.Verify(r => r.Add(It.Is<UserApplicationAssignment>(a => a.IsActive && a.Roles.Contains(role))), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _organisationPersonSyncRepository.Verify(r => r.UpsertAsync(It.IsAny<Guid>(), membership.CdpPersonId!.Value, It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignUser_WhenDuplicate_ThrowsDuplicateEntityException()
    {
        var membership = GivenMembership(Guid.NewGuid(), 42);
        var orgApp = new OrganisationApplication { Id = 5, OrganisationId = 42, ApplicationId = 1, IsActive = true };
        var existing = new UserApplicationAssignment { Id = 1, IsActive = true };

        _membershipRepository.Setup(r => r.GetByPersonIdAndOrganisationAsync(membership.CdpPersonId!.Value, 42, It.IsAny<CancellationToken>())).ReturnsAsync(membership);
        _organisationApplicationRepository.Setup(r => r.GetByOrganisationAndApplicationAsync(42, 1, It.IsAny<CancellationToken>())).ReturnsAsync(orgApp);
        _assignmentRepository.Setup(r => r.GetByMembershipAndApplicationAsync(membership.Id, orgApp.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var act = () => CreateSut().AssignUserToApplicationAsync(membership.CdpPersonId!.Value.ToString(), 42, 1, [1]);

        await act.Should().ThrowAsync<DuplicateEntityException>();
    }

    // ── RevokeApplicationAssignment ─────────────────────────────────────────

    [Fact]
    public async Task RevokeAssignment_HappyPath_DeactivatesAndSyncsOi()
    {
        var membership = GivenMembership(Guid.NewGuid(), 42);
        var assignment = new UserApplicationAssignment
        {
            Id = 99,
            UserOrganisationMembershipId = membership.Id,
            UserOrganisationMembership = membership,
            IsActive = true,
            OrganisationApplication = new OrganisationApplication
            {
                Id = 5,
                ApplicationId = 1,
                Application = new Application { Id = 1, ClientId = "test", IsEnabledByDefault = false }
            }
        };

        _membershipRepository.Setup(r => r.GetByPersonIdAndOrganisationAsync(membership.CdpPersonId!.Value, 42, It.IsAny<CancellationToken>())).ReturnsAsync(membership);
        _assignmentRepository.Setup(r => r.GetByMembershipIdAsync(membership.Id, It.IsAny<CancellationToken>())).ReturnsAsync([assignment]);
        _organisationRepository.Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>())).ReturnsAsync(new CoreOrganisation { Id = 42, CdpOrganisationGuid = Guid.NewGuid(), Name = "Acme", Slug = "acme", IsActive = true, CreatedBy = "seed" });

        await CreateSut().RevokeApplicationAssignmentAsync(membership.CdpPersonId!.Value.ToString(), 42, 99);

        assignment.IsActive.Should().BeFalse();
        assignment.RevokedAt.Should().NotBeNull();
        _assignmentRepository.Verify(r => r.Update(assignment), Times.Once);
    }

    [Fact]
    public async Task RevokeAssignment_WhenDefaultApp_ThrowsInvalidOperationException()
    {
        var membership = GivenMembership(Guid.NewGuid(), 42);
        var assignment = new UserApplicationAssignment
        {
            Id = 99,
            UserOrganisationMembershipId = membership.Id,
            UserOrganisationMembership = membership,
            IsActive = true,
            OrganisationApplication = new OrganisationApplication
            {
                Id = 5,
                ApplicationId = 1,
                Application = new Application { Id = 1, ClientId = "test", IsEnabledByDefault = true }
            }
        };

        _membershipRepository.Setup(r => r.GetByPersonIdAndOrganisationAsync(membership.CdpPersonId!.Value, 42, It.IsAny<CancellationToken>())).ReturnsAsync(membership);
        _assignmentRepository.Setup(r => r.GetByMembershipIdAsync(membership.Id, It.IsAny<CancellationToken>())).ReturnsAsync([assignment]);

        var act = () => CreateSut().RevokeApplicationAssignmentAsync(membership.CdpPersonId!.Value.ToString(), 42, 99);

        await act.Should().ThrowAsync<System.InvalidOperationException>().WithMessage("*enabled by default*");
    }

    // ── AcceptInvite ────────────────────────────────────────────────────────

    [Fact]
    public async Task AcceptInvite_HappyPath_CreatesMembershipAndSyncsOi()
    {
        var orgGuid = Guid.NewGuid();
        var org = GivenOrganisation(orgGuid);
        var mapping = new InviteRoleMapping
        {
            Id = 7,
            OrganisationId = org.Id,
            CdpPersonInviteGuid = Guid.NewGuid(),
            CreatedBy = "inviter",
            OrganisationRoleId = (int)OrganisationRole.Member
        };
        var request = new AcceptOrganisationInviteRequest
        {
            UserPrincipalId = "urn:fdc:gov.uk:2022:new-user",
            CdpPersonId = Guid.NewGuid()
        };

        _organisationRepository.Setup(r => r.GetByCdpGuidAsync(orgGuid, It.IsAny<CancellationToken>())).ReturnsAsync(org);
        _inviteRoleMappingRepository.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(mapping);
        _membershipRepository.Setup(r => r.GetByUserAndOrganisationAsync(request.UserPrincipalId, org.Id, It.IsAny<CancellationToken>())).ReturnsAsync((UserOrganisationMembership?)null);
        _roleMappingService.Setup(r => r.ApplyRoleDefinitionAsync(It.IsAny<UserOrganisationMembership>(), mapping.OrganisationRole, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _roleMappingService.Setup(r => r.ShouldAutoAssignDefaultApplicationsAsync(It.IsAny<UserOrganisationMembership>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _roleMappingService.Setup(r => r.GetOrganisationInformationScopesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(["VIEWER"]);

        await CreateSut().AcceptInviteAsync(orgGuid, 7, request);

        _membershipRepository.Verify(r => r.Add(It.Is<UserOrganisationMembership>(m =>
            m.UserPrincipalId == request.UserPrincipalId &&
            m.CdpPersonId == request.CdpPersonId &&
            m.IsActive)), Times.Once);
        _inviteRoleMappingRepository.Verify(r => r.Remove(mapping), Times.Once);
        _organisationPersonSyncRepository.Verify(r => r.UpsertAsync(orgGuid, request.CdpPersonId, It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AcceptInvite_WhenDuplicateMembership_ThrowsDuplicateEntityException()
    {
        var orgGuid = Guid.NewGuid();
        var org = GivenOrganisation(orgGuid);
        var mapping = new InviteRoleMapping { Id = 7, OrganisationId = org.Id, CdpPersonInviteGuid = Guid.NewGuid(), CreatedBy = "x" };
        var request = new AcceptOrganisationInviteRequest { UserPrincipalId = "existing", CdpPersonId = Guid.NewGuid() };

        _organisationRepository.Setup(r => r.GetByCdpGuidAsync(orgGuid, It.IsAny<CancellationToken>())).ReturnsAsync(org);
        _inviteRoleMappingRepository.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(mapping);
        _membershipRepository.Setup(r => r.GetByUserAndOrganisationAsync("existing", org.Id, It.IsAny<CancellationToken>())).ReturnsAsync(GivenMembership(Guid.NewGuid(), org.Id));

        var act = () => CreateSut().AcceptInviteAsync(orgGuid, 7, request);

        await act.Should().ThrowAsync<DuplicateEntityException>();
    }

    [Fact]
    public async Task AcceptInvite_WhenMappingNotFound_ThrowsEntityNotFoundException()
    {
        var orgGuid = Guid.NewGuid();
        var org = GivenOrganisation(orgGuid);
        var request = new AcceptOrganisationInviteRequest { UserPrincipalId = "x", CdpPersonId = Guid.NewGuid() };

        _organisationRepository.Setup(r => r.GetByCdpGuidAsync(orgGuid, It.IsAny<CancellationToken>())).ReturnsAsync(org);
        _inviteRoleMappingRepository.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync((InviteRoleMapping?)null);

        var act = () => CreateSut().AcceptInviteAsync(orgGuid, 7, request);

        await act.Should().ThrowAsync<EntityNotFoundException>().WithMessage("*InviteRoleMapping*");
    }
}
