using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Services;
using CO.CDP.UserManagement.Shared.Enums;
using FluentAssertions;
using Moq;
using CoreOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Api.Tests.Services;

public class MembershipAuthorizationGuardTests
{
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly Mock<IOrganisationRepository> _organisationRepository = new();
    private readonly Mock<IUserOrganisationMembershipRepository> _membershipRepository = new();

    private const string ActorPrincipalId = "urn:fdc:gov.uk:2022:actor";
    private const string TargetPrincipalId = "urn:fdc:gov.uk:2022:target";

    private MembershipAuthorizationGuard CreateSut() => new(
        _currentUserService.Object,
        _organisationRepository.Object,
        _membershipRepository.Object);

    private CoreOrganisation GivenOrganisation(Guid cdpGuid)
    {
        var org = new CoreOrganisation
        {
            Id = 1,
            CdpOrganisationGuid = cdpGuid,
            Name = "Org",
            Slug = "org",
            IsActive = true,
            CreatedBy = "seed"
        };
        _organisationRepository
            .Setup(r => r.GetByCdpGuidAsync(cdpGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        return org;
    }

    private void GivenActorMembership(int organisationId, OrganisationRole role) =>
        _membershipRepository
            .Setup(r => r.GetByUserAndOrganisationAsync(ActorPrincipalId, organisationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserOrganisationMembership
            {
                UserPrincipalId = ActorPrincipalId,
                OrganisationRoleId = (int)role,
                JoinedAt = DateTimeOffset.UtcNow,
                CreatedBy = "seed"
            });

    private void GivenTargetMembership(Guid personId, int organisationId, OrganisationRole role) =>
        _membershipRepository
            .Setup(r => r.GetByPersonIdAndOrganisationAsync(personId, organisationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserOrganisationMembership
            {
                UserPrincipalId = TargetPrincipalId,
                OrganisationRoleId = (int)role,
                JoinedAt = DateTimeOffset.UtcNow,
                CreatedBy = "seed"
            });

    // ── ValidateRemovalAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task ValidateRemoval_WhenActorRemovesSelf_ThrowsForbidden()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var org = GivenOrganisation(orgGuid);

        _currentUserService.Setup(s => s.GetUserPrincipalId()).Returns(ActorPrincipalId);
        GivenActorMembership(org.Id, OrganisationRole.Admin);

        // Target is the same principal as the actor
        _membershipRepository
            .Setup(r => r.GetByPersonIdAndOrganisationAsync(personId, org.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserOrganisationMembership
            {
                UserPrincipalId = ActorPrincipalId,
                OrganisationRoleId = (int)OrganisationRole.Admin,
                JoinedAt = DateTimeOffset.UtcNow,
                CreatedBy = "seed"
            });

        var act = () => CreateSut().ValidateRemovalAsync(orgGuid, personId);

        await act.Should().ThrowAsync<MembershipOperationForbiddenException>();
    }

    [Fact]
    public async Task ValidateRemoval_WhenAdminRemovesOwner_ThrowsForbidden()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var org = GivenOrganisation(orgGuid);

        _currentUserService.Setup(s => s.GetUserPrincipalId()).Returns(ActorPrincipalId);
        GivenActorMembership(org.Id, OrganisationRole.Admin);
        GivenTargetMembership(personId, org.Id, OrganisationRole.Owner);

        var act = () => CreateSut().ValidateRemovalAsync(orgGuid, personId);

        await act.Should().ThrowAsync<MembershipOperationForbiddenException>();
    }

    [Fact]
    public async Task ValidateRemoval_WhenOwnerRemovesMember_Succeeds()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var org = GivenOrganisation(orgGuid);

        _currentUserService.Setup(s => s.GetUserPrincipalId()).Returns(ActorPrincipalId);
        GivenActorMembership(org.Id, OrganisationRole.Owner);
        GivenTargetMembership(personId, org.Id, OrganisationRole.Member);

        var act = () => CreateSut().ValidateRemovalAsync(orgGuid, personId);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ValidateRemoval_WhenActorRoleResolvedFromRepository_NotFromClaims()
    {
        // Simulates stale JWT: GetOrganisationRole() claim says Admin, but repo says Owner.
        // Guard should use repo value (Owner), which allows removing a Member.
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var org = GivenOrganisation(orgGuid);

        _currentUserService.Setup(s => s.GetUserPrincipalId()).Returns(ActorPrincipalId);
        // Stale claim would say Admin — but guard ignores claims for role resolution:
        _currentUserService.Setup(s => s.GetOrganisationRole(orgGuid)).Returns(OrganisationRole.Admin);
        // Repository says Owner (live):
        GivenActorMembership(org.Id, OrganisationRole.Owner);
        GivenTargetMembership(personId, org.Id, OrganisationRole.Member);

        var act = () => CreateSut().ValidateRemovalAsync(orgGuid, personId);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ValidateRemoval_WhenCurrentUserPrincipalIdNull_ThrowsForbidden()
    {
        _currentUserService.Setup(s => s.GetUserPrincipalId()).Returns((string?)null);

        var act = () => CreateSut().ValidateRemovalAsync(Guid.NewGuid(), Guid.NewGuid());

        await act.Should().ThrowAsync<MembershipOperationForbiddenException>();
    }

    [Fact]
    public async Task ValidateRemoval_WhenOrganisationNotFound_ThrowsEntityNotFound()
    {
        var orgGuid = Guid.NewGuid();
        _currentUserService.Setup(s => s.GetUserPrincipalId()).Returns(ActorPrincipalId);
        _organisationRepository
            .Setup(r => r.GetByCdpGuidAsync(orgGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CoreOrganisation?)null);

        var act = () => CreateSut().ValidateRemovalAsync(orgGuid, Guid.NewGuid());

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // ── ValidateRoleChangeAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task ValidateRoleChange_WhenActorChangesOwnRole_ThrowsForbidden()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var org = GivenOrganisation(orgGuid);

        _currentUserService.Setup(s => s.GetUserPrincipalId()).Returns(ActorPrincipalId);
        GivenActorMembership(org.Id, OrganisationRole.Admin);

        // Target has same principal ID as actor
        _membershipRepository
            .Setup(r => r.GetByPersonIdAndOrganisationAsync(personId, org.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserOrganisationMembership
            {
                UserPrincipalId = ActorPrincipalId,
                OrganisationRoleId = (int)OrganisationRole.Admin,
                JoinedAt = DateTimeOffset.UtcNow,
                CreatedBy = "seed"
            });

        var act = () => CreateSut().ValidateRoleChangeAsync(orgGuid, personId, OrganisationRole.Member);

        await act.Should().ThrowAsync<MembershipOperationForbiddenException>();
    }

    [Fact]
    public async Task ValidateRoleChange_WhenAdminChangesOwnerRole_ThrowsForbidden()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var org = GivenOrganisation(orgGuid);

        _currentUserService.Setup(s => s.GetUserPrincipalId()).Returns(ActorPrincipalId);
        GivenActorMembership(org.Id, OrganisationRole.Admin);
        GivenTargetMembership(personId, org.Id, OrganisationRole.Owner);

        var act = () => CreateSut().ValidateRoleChangeAsync(orgGuid, personId, OrganisationRole.Member);

        await act.Should().ThrowAsync<MembershipOperationForbiddenException>();
    }

    [Fact]
    public async Task ValidateRoleChange_WhenOwnerChangesMemberRole_Succeeds()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var org = GivenOrganisation(orgGuid);

        _currentUserService.Setup(s => s.GetUserPrincipalId()).Returns(ActorPrincipalId);
        GivenActorMembership(org.Id, OrganisationRole.Owner);
        GivenTargetMembership(personId, org.Id, OrganisationRole.Member);

        var act = () => CreateSut().ValidateRoleChangeAsync(orgGuid, personId, OrganisationRole.Admin);

        await act.Should().NotThrowAsync();
    }
}
