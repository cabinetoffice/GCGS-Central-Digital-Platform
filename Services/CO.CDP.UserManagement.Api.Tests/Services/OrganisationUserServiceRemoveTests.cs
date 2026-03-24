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

    private OrganisationUserService CreateSut() => new(
        _organisationRepository.Object,
        _membershipRepository.Object,
        _assignmentRepository.Object,
        _atomicMembershipSync.Object,
        NullLogger<OrganisationUserService>.Instance);

    [Fact]
    public async Task RemoveUser_DelegatesToAtomicMembershipSync()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();

        _atomicMembershipSync
            .Setup(s => s.RemoveUserFromOrganisationAsync(orgGuid, personId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await CreateSut().RemoveUserFromOrganisationAsync(orgGuid, personId);

        _atomicMembershipSync.Verify(
            s => s.RemoveUserFromOrganisationAsync(orgGuid, personId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveUser_PropagatesEntityNotFoundException()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();

        _atomicMembershipSync
            .Setup(s => s.RemoveUserFromOrganisationAsync(orgGuid, personId, It.IsAny<CancellationToken>()))
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
            .Setup(s => s.RemoveUserFromOrganisationAsync(orgGuid, personId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LastOwnerRemovalException(orgGuid));

        var act = () => CreateSut().RemoveUserFromOrganisationAsync(orgGuid, personId);

        await act.Should().ThrowAsync<LastOwnerRemovalException>();
    }

    [Fact]
    public async Task UpdateOrganisationRole_DelegatesToAtomicMembershipSync()
    {
        var orgGuid = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var expectedMembership = new UserOrganisationMembership { Id = 1 };

        _atomicMembershipSync
            .Setup(s => s.UpdateMembershipRoleAsync(orgGuid, personId, OrganisationRole.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedMembership);

        var result = await CreateSut().UpdateOrganisationRoleAsync(orgGuid, personId, OrganisationRole.Admin);

        result.Should().Be(expectedMembership);
        _atomicMembershipSync.Verify(
            s => s.UpdateMembershipRoleAsync(orgGuid, personId, OrganisationRole.Admin, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
