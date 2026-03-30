using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.UserManagement.Api.Tests.Services;

public class UserAssignmentServiceTests
{
    private readonly Mock<IUserApplicationAssignmentRepository> _assignmentRepository = new();
    private readonly Mock<IUserOrganisationMembershipRepository> _membershipRepository = new();
    private readonly Mock<IAtomicMembershipSync> _atomicMembershipSync = new();
    private readonly Mock<ILogger<UserAssignmentService>> _logger = new();

    private UserAssignmentService CreateSut() => new(
        _assignmentRepository.Object,
        _membershipRepository.Object,
        _atomicMembershipSync.Object,
        _logger.Object);

    [Fact]
    public async Task GetUserAssignmentsAsync_ReturnsAssignments()
    {
        var personId = Guid.NewGuid();
        var membership = new UserOrganisationMembership { Id = 10, CdpPersonId = personId, OrganisationId = 42, IsActive = true };
        var assignments = new List<UserApplicationAssignment> { new() { Id = 1, IsActive = true } };

        _membershipRepository.Setup(r => r.GetByPersonIdAndOrganisationAsync(personId, 42, It.IsAny<CancellationToken>())).ReturnsAsync(membership);
        _assignmentRepository.Setup(r => r.GetByMembershipIdAsync(membership.Id, It.IsAny<CancellationToken>())).ReturnsAsync(assignments);

        var result = await CreateSut().GetUserAssignmentsAsync(personId.ToString(), 42);

        result.Should().BeEquivalentTo(assignments);
    }

    [Fact]
    public async Task GetUserAssignmentsAsync_WhenMembershipNotFound_ThrowsEntityNotFoundException()
    {
        var personId = Guid.NewGuid();
        _membershipRepository.Setup(r => r.GetByPersonIdAndOrganisationAsync(personId, 42, It.IsAny<CancellationToken>())).ReturnsAsync((UserOrganisationMembership?)null);

        var act = () => CreateSut().GetUserAssignmentsAsync(personId.ToString(), 42);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task AssignUserAsync_DelegatesToAtomicMembershipSync()
    {
        var expected = new UserApplicationAssignment { Id = 5, IsActive = true };
        _atomicMembershipSync
            .Setup(s => s.AssignUserToApplicationAsync("user1", 42, 1, It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await CreateSut().AssignUserAsync("user1", 42, 1, [1]);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task UpdateAssignmentAsync_DelegatesToAtomicMembershipSync()
    {
        var expected = new UserApplicationAssignment { Id = 5, IsActive = true };
        _atomicMembershipSync
            .Setup(s => s.UpdateApplicationAssignmentAsync("user1", 42, 5, It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await CreateSut().UpdateAssignmentAsync("user1", 42, 5, [1]);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task RevokeAssignmentAsync_DelegatesToAtomicMembershipSync()
    {
        await CreateSut().RevokeAssignmentAsync("user1", 42, 5);

        _atomicMembershipSync.Verify(s => s.RevokeApplicationAssignmentAsync("user1", 42, 5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignDefaultApplicationsAsync_IsNoOp()
    {
        var membership = new UserOrganisationMembership { Id = 10, OrganisationId = 42 };

        await CreateSut().AssignDefaultApplicationsAsync(membership);

        _atomicMembershipSync.VerifyNoOtherCalls();
    }
}
