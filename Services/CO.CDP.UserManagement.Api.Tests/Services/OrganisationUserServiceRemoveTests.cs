using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.UseCase;
using CO.CDP.UserManagement.Infrastructure.Services;
using CO.CDP.UserManagement.Infrastructure.UseCase.RemovePersonFromOrganisation;
using CO.CDP.UserManagement.Infrastructure.UseCase.UpdateOrganisationRole;
using CO.CDP.UserManagement.Shared.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using CoreOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Api.Tests.Services;

public class OrganisationUserServiceRemoveTests
{
    private readonly Mock<IUserApplicationAssignmentRepository> _assignmentRepository = new();
    private readonly Mock<IMembershipAuthorizationGuard> _authorizationGuard = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly Mock<IUserOrganisationMembershipRepository> _membershipRepository = new();
    private readonly Mock<IOrganisationRepository> _organisationRepository = new();
    private readonly Mock<IUseCase<RemovePersonFromOrganisationCommand>> _removePersonUseCase = new();

    private readonly Mock<IUseCase<UpdateOrganisationRoleCommand, UserOrganisationMembership>> _updateRoleUseCase =
        new();

    public OrganisationUserServiceRemoveTests()
    {
        _authorizationGuard.Setup(g =>
                g.ValidateRemovalAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _authorizationGuard.Setup(g => g.ValidateRoleChangeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<OrganisationRole>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _currentUserService.Setup(s => s.GetUserPrincipalId()).Returns("urn:fdc:gov.uk:2022:test-actor");
        _removePersonUseCase.Setup(s =>
                s.Execute(It.IsAny<RemovePersonFromOrganisationCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _updateRoleUseCase.Setup(s =>
                s.Execute(It.IsAny<UpdateOrganisationRoleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserOrganisationMembership());
    }

    private OrganisationUserService CreateSut() => new(
        _organisationRepository.Object,
        _membershipRepository.Object,
        _assignmentRepository.Object,
        _removePersonUseCase.Object,
        _updateRoleUseCase.Object,
        _authorizationGuard.Object,
        _currentUserService.Object,
        NullLogger<OrganisationUserService>.Instance);

    [Fact]
    public async Task RemoveUser_RunsGuardBeforeDelegatingToUseCase()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var callOrder = new List<string>();

        _authorizationGuard
            .Setup(g => g.ValidateRemovalAsync(orgGuid, personId, It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("guard"))
            .Returns(Task.CompletedTask);
        _removePersonUseCase
            .Setup(s => s.Execute(It.IsAny<RemovePersonFromOrganisationCommand>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("usecase"))
            .Returns(Task.CompletedTask);

        await CreateSut().RemoveUserFromOrganisationAsync(orgGuid, personId);

        callOrder.Should().Equal("guard", "usecase");
    }

    [Fact]
    public async Task RemoveUser_DelegatesToRemovePersonUseCase()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();

        await CreateSut().RemoveUserFromOrganisationAsync(orgGuid, personId);

        _removePersonUseCase.Verify(s => s.Execute(
            It.Is<RemovePersonFromOrganisationCommand>(c =>
                c.CdpOrganisationId == orgGuid && c.CdpPersonId == personId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveUser_PassesActingUserIdFromCurrentUserService()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        const string actingUser = "urn:fdc:gov.uk:2022:test-actor";
        _currentUserService.Setup(s => s.GetUserPrincipalId()).Returns(actingUser);

        await CreateSut().RemoveUserFromOrganisationAsync(orgGuid, personId);

        _removePersonUseCase.Verify(s => s.Execute(
            It.Is<RemovePersonFromOrganisationCommand>(c => c.ActingUserId == actingUser),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveUser_WhenGuardThrows_DoesNotCallUseCase()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();

        _authorizationGuard
            .Setup(g => g.ValidateRemovalAsync(orgGuid, personId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MembershipOperationForbiddenException("Not allowed."));

        var act = () => CreateSut().RemoveUserFromOrganisationAsync(orgGuid, personId);

        await act.Should().ThrowAsync<MembershipOperationForbiddenException>();
        _removePersonUseCase.Verify(
            s => s.Execute(It.IsAny<RemovePersonFromOrganisationCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RemoveUser_PropagatesEntityNotFoundException()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();

        _removePersonUseCase
            .Setup(s => s.Execute(It.IsAny<RemovePersonFromOrganisationCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException(nameof(CoreOrganisation), orgGuid));

        var act = () => CreateSut().RemoveUserFromOrganisationAsync(orgGuid, personId);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task RemoveUser_PropagatesLastOwnerRemovalException()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();

        _removePersonUseCase
            .Setup(s => s.Execute(It.IsAny<RemovePersonFromOrganisationCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LastOwnerRemovalException(orgGuid));

        var act = () => CreateSut().RemoveUserFromOrganisationAsync(orgGuid, personId);

        await act.Should().ThrowAsync<LastOwnerRemovalException>();
    }

    [Fact]
    public async Task UpdateOrganisationRole_RunsGuardBeforeDelegatingToUseCase()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var callOrder = new List<string>();
        var expectedMembership = new UserOrganisationMembership { Id = 1 };

        _authorizationGuard
            .Setup(g => g.ValidateRoleChangeAsync(orgGuid, personId, OrganisationRole.Admin,
                It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("guard"))
            .Returns(Task.CompletedTask);
        _updateRoleUseCase
            .Setup(s => s.Execute(It.IsAny<UpdateOrganisationRoleCommand>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("usecase"))
            .ReturnsAsync(expectedMembership);

        await CreateSut().UpdateOrganisationRoleAsync(orgGuid, personId, OrganisationRole.Admin);

        callOrder.Should().Equal("guard", "usecase");
    }

    [Fact]
    public async Task UpdateOrganisationRole_DelegatesToUpdateRoleUseCase()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var expectedMembership = new UserOrganisationMembership { Id = 1 };

        _updateRoleUseCase
            .Setup(s => s.Execute(It.IsAny<UpdateOrganisationRoleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedMembership);

        var result = await CreateSut().UpdateOrganisationRoleAsync(orgGuid, personId, OrganisationRole.Admin);

        result.Should().Be(expectedMembership);
        _updateRoleUseCase.Verify(s => s.Execute(
            It.Is<UpdateOrganisationRoleCommand>(c =>
                c.CdpOrganisationId == orgGuid && c.CdpPersonId == personId && c.NewRole == OrganisationRole.Admin),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}