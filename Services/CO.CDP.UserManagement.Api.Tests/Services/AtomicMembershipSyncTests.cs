using CO.CDP.OrganisationSync;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using CoreOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Api.Tests.Services;

public class AtomicMembershipSyncTests
{
    private readonly Mock<IAtomicScope> _atomicScope = new();
    private readonly Mock<IOrganisationRepository> _organisationRepository = new();
    private readonly Mock<IUserOrganisationMembershipRepository> _membershipRepository = new();
    private readonly Mock<IUserApplicationAssignmentRepository> _assignmentRepository = new();
    private readonly Mock<IRoleMappingService> _roleMappingService = new();
    private readonly Mock<IOrganisationPersonSyncRepository> _organisationPersonSyncRepository = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();

    public AtomicMembershipSyncTests()
    {
        _atomicScope
            .Setup(s => s.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<CO.CDP.Functional.Unit>>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<CO.CDP.Functional.Unit>>, CancellationToken>((action, ct) => action(ct));

        _atomicScope
            .Setup(s => s.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<UserOrganisationMembership>>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<UserOrganisationMembership>>, CancellationToken>((action, ct) => action(ct));

        _currentUserService.Setup(s => s.GetUserPrincipalId()).Returns("urn:fdc:gov.uk:2022:test-actor");
    }

    private AtomicMembershipSync CreateSut() => new(
        _atomicScope.Object,
        _organisationRepository.Object,
        _membershipRepository.Object,
        _assignmentRepository.Object,
        _roleMappingService.Object,
        _organisationPersonSyncRepository.Object,
        _currentUserService.Object,
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
        _roleMappingService.Setup(r => r.ShouldSyncToOrganisationInformationAsync(membership.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _roleMappingService.Setup(r => r.GetOrganisationInformationScopesAsync(membership.Id, It.IsAny<CancellationToken>())).ReturnsAsync(["ADMIN"]);

        var result = await CreateSut().UpdateMembershipRoleAsync(orgGuid, personId, OrganisationRole.Admin);

        result.Should().Be(membership);
        _membershipRepository.Verify(r => r.Update(membership), Times.Once);
        _organisationPersonSyncRepository.Verify(
            r => r.UpsertAsync(org.CdpOrganisationGuid, personId, It.Is<IReadOnlyList<string>>(s => s.Contains("ADMIN")), It.IsAny<CancellationToken>()),
            Times.Once);
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
        _roleMappingService.Setup(r => r.ShouldSyncToOrganisationInformationAsync(membership.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

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
        _roleMappingService.Setup(r => r.ShouldSyncToOrganisationInformationAsync(membership.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await CreateSut().UpdateMembershipRoleAsync(orgGuid, personId, OrganisationRole.Admin);

        _atomicScope.Verify(
            s => s.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<UserOrganisationMembership>>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
