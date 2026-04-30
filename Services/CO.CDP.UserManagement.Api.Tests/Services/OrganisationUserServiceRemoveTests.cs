using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Services;
using CO.CDP.UserManagement.Shared.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using CoreOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Api.Tests.Services;

public class OrganisationUserServiceRemoveTests
{
    private readonly Mock<IOrganisationRepository> _organisationRepository = new();
    private readonly Mock<IUserOrganisationMembershipRepository> _membershipRepository = new();
    private readonly Mock<IUserApplicationAssignmentRepository> _assignmentRepository = new();
    private readonly Mock<IAtomicMembershipSync> _atomicMembershipSync = new();
    private readonly Mock<IMembershipAuthorizationGuard> _authorizationGuard = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();

    public OrganisationUserServiceRemoveTests()
    {
        _authorizationGuard.Setup(g => g.ValidateRemovalAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _authorizationGuard.Setup(g => g.ValidateRoleChangeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<OrganisationRole>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _currentUserService.Setup(s => s.GetUserPrincipalId()).Returns("urn:fdc:gov.uk:2022:test-actor");
    }

    private OrganisationUserService CreateSut() => new(
        _organisationRepository.Object,
        _membershipRepository.Object,
        _assignmentRepository.Object,
        _atomicMembershipSync.Object,
        _authorizationGuard.Object,
        _currentUserService.Object,
        NullLogger<OrganisationUserService>.Instance);

    [Fact]
    public async Task RemoveUser_RunsGuardBeforeDelegatingToSync()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var callOrder = new List<string>();

        _authorizationGuard
            .Setup(g => g.ValidateRemovalAsync(orgGuid, personId, It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("guard"))
            .Returns(Task.CompletedTask);
        _atomicMembershipSync
            .Setup(s => s.RemoveUserFromOrganisationAsync(orgGuid, personId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("sync"))
            .Returns(Task.CompletedTask);

        await CreateSut().RemoveUserFromOrganisationAsync(orgGuid, personId);

        callOrder.Should().Equal("guard", "sync");
    }

    [Fact]
    public async Task RemoveUser_DelegatesToAtomicMembershipSync()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();

        _atomicMembershipSync
            .Setup(s => s.RemoveUserFromOrganisationAsync(orgGuid, personId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await CreateSut().RemoveUserFromOrganisationAsync(orgGuid, personId);

        _atomicMembershipSync.Verify(
            s => s.RemoveUserFromOrganisationAsync(orgGuid, personId, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveUser_PassesActingUserIdFromCurrentUserService()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        const string actingUser = "urn:fdc:gov.uk:2022:test-actor";
        _currentUserService.Setup(s => s.GetUserPrincipalId()).Returns(actingUser);

        _atomicMembershipSync
            .Setup(s => s.RemoveUserFromOrganisationAsync(orgGuid, personId, actingUser, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await CreateSut().RemoveUserFromOrganisationAsync(orgGuid, personId);

        _atomicMembershipSync.Verify(
            s => s.RemoveUserFromOrganisationAsync(orgGuid, personId, actingUser, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveUser_WhenGuardThrows_DoesNotCallSync()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();

        _authorizationGuard
            .Setup(g => g.ValidateRemovalAsync(orgGuid, personId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MembershipOperationForbiddenException("Not allowed."));

        var act = () => CreateSut().RemoveUserFromOrganisationAsync(orgGuid, personId);

        await act.Should().ThrowAsync<MembershipOperationForbiddenException>();
        _atomicMembershipSync.Verify(
            s => s.RemoveUserFromOrganisationAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RemoveUser_PropagatesEntityNotFoundException()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();

        _atomicMembershipSync
            .Setup(s => s.RemoveUserFromOrganisationAsync(orgGuid, personId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException(nameof(CoreOrganisation), orgGuid));

        var act = () => CreateSut().RemoveUserFromOrganisationAsync(orgGuid, personId);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task RemoveUser_PropagatesLastOwnerRemovalException()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();

        _atomicMembershipSync
            .Setup(s => s.RemoveUserFromOrganisationAsync(orgGuid, personId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LastOwnerRemovalException(orgGuid));

        var act = () => CreateSut().RemoveUserFromOrganisationAsync(orgGuid, personId);

        await act.Should().ThrowAsync<LastOwnerRemovalException>();
    }

    [Fact]
    public async Task UpdateOrganisationRole_RunsGuardBeforeDelegatingToSync()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var callOrder = new List<string>();
        var expectedMembership = new UserOrganisationMembership { Id = 1 };

        _authorizationGuard
            .Setup(g => g.ValidateRoleChangeAsync(orgGuid, personId, OrganisationRole.Admin, It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("guard"))
            .Returns(Task.CompletedTask);
        _atomicMembershipSync
            .Setup(s => s.UpdateMembershipRoleAsync(orgGuid, personId, OrganisationRole.Admin, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("sync"))
            .ReturnsAsync(expectedMembership);

        await CreateSut().UpdateOrganisationRoleAsync(orgGuid, personId, OrganisationRole.Admin);

        callOrder.Should().Equal("guard", "sync");
    }

    [Fact]
    public async Task UpdateOrganisationRole_DelegatesToAtomicMembershipSync()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var expectedMembership = new UserOrganisationMembership { Id = 1 };

        _atomicMembershipSync
            .Setup(s => s.UpdateMembershipRoleAsync(orgGuid, personId, OrganisationRole.Admin, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedMembership);

        var result = await CreateSut().UpdateOrganisationRoleAsync(orgGuid, personId, OrganisationRole.Admin);

        result.Should().Be(expectedMembership);
        _atomicMembershipSync.Verify(
            s => s.UpdateMembershipRoleAsync(orgGuid, personId, OrganisationRole.Admin, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
