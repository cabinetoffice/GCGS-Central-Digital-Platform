using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.UseCase;
using CO.CDP.UserManagement.Infrastructure.Services;
using CO.CDP.UserManagement.Infrastructure.UseCase.AssignUserToApplication;
using CO.CDP.UserManagement.Infrastructure.UseCase.RevokeApplicationAssignment;
using CO.CDP.UserManagement.Infrastructure.UseCase.UpdateApplicationAssignment;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.UserManagement.Api.Tests.Services;

public class UserAssignmentServiceTests
{
    private readonly Mock<IUserApplicationAssignmentRepository> _assignmentRepository = new();
    private readonly Mock<IUseCase<AssignUserToApplicationCommand, UserApplicationAssignment>> _assignUseCase = new();
    private readonly Mock<ILogger<UserAssignmentService>> _logger = new();
    private readonly Mock<IUserOrganisationMembershipRepository> _membershipRepository = new();
    private readonly Mock<IUseCase<RevokeApplicationAssignmentCommand>> _revokeAssignmentUseCase = new();

    private readonly Mock<IUseCase<UpdateApplicationAssignmentCommand, UserApplicationAssignment>>
        _updateAssignmentUseCase = new();

    private UserAssignmentService CreateSut() => new(
        _assignmentRepository.Object,
        _membershipRepository.Object,
        _assignUseCase.Object,
        _updateAssignmentUseCase.Object,
        _revokeAssignmentUseCase.Object,
        _logger.Object);

    [Fact]
    public async Task GetUserAssignmentsAsync_ReturnsAssignments()
    {
        var personId = Guid.NewGuid();
        var membership = new UserOrganisationMembership
            { Id = 10, CdpPersonId = personId, OrganisationId = 42, IsActive = true };
        var assignments = new List<UserApplicationAssignment> { new() { Id = 1, IsActive = true } };

        _membershipRepository
            .Setup(r => r.GetByPersonIdAndOrganisationAsync(personId, 42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);
        _assignmentRepository.Setup(r => r.GetByMembershipIdAsync(membership.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignments);

        var result = await CreateSut().GetUserAssignmentsAsync(personId.ToString(), 42);

        result.Should().BeEquivalentTo(assignments);
    }

    [Fact]
    public async Task GetUserAssignmentsAsync_WhenMembershipNotFound_ThrowsEntityNotFoundException()
    {
        var personId = Guid.NewGuid();
        _membershipRepository
            .Setup(r => r.GetByPersonIdAndOrganisationAsync(personId, 42, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserOrganisationMembership?)null);

        var act = () => CreateSut().GetUserAssignmentsAsync(personId.ToString(), 42);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task AssignUserAsync_DelegatesToUseCase()
    {
        var expected = new UserApplicationAssignment { Id = 5, IsActive = true };
        _assignUseCase
            .Setup(s => s.Execute(It.IsAny<AssignUserToApplicationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await CreateSut().AssignUserAsync("user1", 42, 1, [1]);

        result.Should().Be(expected);
        _assignUseCase.Verify(s => s.Execute(
            It.Is<AssignUserToApplicationCommand>(c =>
                c.UserId == "user1" && c.OrganisationId == 42 && c.ApplicationId == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAssignmentAsync_DelegatesToUseCase()
    {
        var expected = new UserApplicationAssignment { Id = 5, IsActive = true };
        _updateAssignmentUseCase
            .Setup(s => s.Execute(It.IsAny<UpdateApplicationAssignmentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await CreateSut().UpdateAssignmentAsync("user1", 42, 5, [1]);

        result.Should().Be(expected);
        _updateAssignmentUseCase.Verify(s => s.Execute(
            It.Is<UpdateApplicationAssignmentCommand>(c =>
                c.UserId == "user1" && c.OrganisationId == 42 && c.AssignmentId == 5),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeAssignmentAsync_DelegatesToUseCase()
    {
        _revokeAssignmentUseCase
            .Setup(s => s.Execute(It.IsAny<RevokeApplicationAssignmentCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await CreateSut().RevokeAssignmentAsync("user1", 42, 5);

        _revokeAssignmentUseCase.Verify(s => s.Execute(
            It.Is<RevokeApplicationAssignmentCommand>(c =>
                c.UserId == "user1" && c.OrganisationId == 42 && c.AssignmentId == 5),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignDefaultApplicationsAsync_IsNoOp()
    {
        var membership = new UserOrganisationMembership { Id = 10, OrganisationId = 42 };

        await CreateSut().AssignDefaultApplicationsAsync(membership);

        _assignUseCase.VerifyNoOtherCalls();
        _updateAssignmentUseCase.VerifyNoOtherCalls();
        _revokeAssignmentUseCase.VerifyNoOtherCalls();
    }
}