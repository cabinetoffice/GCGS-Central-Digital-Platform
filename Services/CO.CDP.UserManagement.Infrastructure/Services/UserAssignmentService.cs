using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CO.CDP.UserManagement.Infrastructure.Services;

/// <summary>
/// Thin delegator for user application assignments.
/// Read operations query locally; write operations delegate to
/// <see cref="IAtomicMembershipSync"/> for atomic cross-DB consistency.
/// </summary>
public class UserAssignmentService(
    IUserApplicationAssignmentRepository assignmentRepository,
    IUserOrganisationMembershipRepository membershipRepository,
    IAtomicMembershipSync atomicMembershipSync,
    ILogger<UserAssignmentService> logger) : IUserAssignmentService
{
    public async Task<IEnumerable<UserApplicationAssignment>> GetUserAssignmentsAsync(
        string userId,
        int organisationId,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting assignments for user: {UserId} in organisation ID: {OrganisationId}",
            userId, organisationId);

        var membership = await ResolveMembershipAsync(userId, organisationId, cancellationToken)
            ?? throw new EntityNotFoundException(
                nameof(UserOrganisationMembership),
                $"User {userId} in Organisation {organisationId}");

        return await assignmentRepository.GetByMembershipIdAsync(membership.Id, cancellationToken);
    }

    public Task<UserApplicationAssignment> AssignUserAsync(
        string userId, int organisationId, int applicationId,
        IEnumerable<int> roleIds, CancellationToken cancellationToken = default) =>
        atomicMembershipSync.AssignUserToApplicationAsync(userId, organisationId, applicationId, roleIds, cancellationToken);

    public Task<UserApplicationAssignment> UpdateAssignmentAsync(
        string userId, int organisationId, int assignmentId,
        IEnumerable<int> roleIds, CancellationToken cancellationToken = default) =>
        atomicMembershipSync.UpdateApplicationAssignmentAsync(userId, organisationId, assignmentId, roleIds, cancellationToken);

    public Task RevokeAssignmentAsync(
        string userId, int organisationId, int assignmentId,
        CancellationToken cancellationToken = default) =>
        atomicMembershipSync.RevokeApplicationAssignmentAsync(userId, organisationId, assignmentId, cancellationToken);

    public Task AssignDefaultApplicationsAsync(
        UserOrganisationMembership membership,
        CancellationToken cancellationToken = default)
    {
        logger.LogWarning(
            "AssignDefaultApplicationsAsync called directly for membership {MembershipId} — " +
            "this is now handled atomically inside IAtomicMembershipSync.AcceptInviteAsync",
            membership.Id);
        return Task.CompletedTask;
    }

    private async Task<UserOrganisationMembership?> ResolveMembershipAsync(
        string userId, int organisationId, CancellationToken cancellationToken) =>
        Guid.TryParse(userId, out var cdpPersonId)
            ? await membershipRepository.GetByPersonIdAndOrganisationAsync(cdpPersonId, organisationId, cancellationToken)
            : await membershipRepository.GetByUserAndOrganisationAsync(userId, organisationId, cancellationToken);
}
