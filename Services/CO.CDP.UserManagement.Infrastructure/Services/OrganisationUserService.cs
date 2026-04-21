using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.UseCase;
using CO.CDP.UserManagement.Infrastructure.UseCase.RemovePersonFromOrganisation;
using CO.CDP.UserManagement.Infrastructure.UseCase.UpdateOrganisationRole;
using CO.CDP.UserManagement.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace CO.CDP.UserManagement.Infrastructure.Services;

/// <summary>
/// Service for managing organisation user memberships.
/// Authorization is enforced by <see cref="IMembershipAuthorizationGuard"/> before
/// write operations delegate to the relevant use case.
/// </summary>
public class OrganisationUserService(
    IOrganisationRepository organisationRepository,
    IUserOrganisationMembershipRepository membershipRepository,
    IUserApplicationAssignmentRepository assignmentRepository,
    IUseCase<RemovePersonFromOrganisationCommand> removePersonUseCase,
    IUseCase<UpdateOrganisationRoleCommand, UserOrganisationMembership> updateRoleUseCase,
    IMembershipAuthorizationGuard authorizationGuard,
    ICurrentUserService currentUserService,
    ILogger<OrganisationUserService> logger) : IOrganisationUserService
{
    public async Task<IEnumerable<UserOrganisationMembership>> GetOrganisationUsersAsync(
        Guid cdpOrganisationId,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting users for CDP organisation ID: {CdpOrganisationId}", cdpOrganisationId);

        var organisation = await organisationRepository.GetByCdpGuidAsync(cdpOrganisationId, cancellationToken)
                           ?? throw new EntityNotFoundException(nameof(Organisation), cdpOrganisationId);

        logger.LogDebug("Resolved organisation {OrganisationId} for CDP organisation ID: {CdpOrganisationId}",
            organisation.Id, cdpOrganisationId);
        var memberships = (await membershipRepository.GetByOrganisationIdAsync(organisation.Id, cancellationToken))
            .ToList();
        logger.LogDebug("Retrieved {MembershipCount} memberships for organisation {OrganisationId}", memberships.Count,
            organisation.Id);

        var membershipIds = memberships.Select(m => m.Id).ToArray();
        var assignments = await assignmentRepository.GetByMembershipIdsAsync(membershipIds, cancellationToken);
        var assignmentsByMembership = assignments.ToLookup(a => a.UserOrganisationMembershipId);

        foreach (var membership in memberships)
            SetAssignments(membership, assignmentsByMembership[membership.Id]);

        return memberships;
    }

    public async Task<UserOrganisationMembership?> GetOrganisationUserAsync(
        Guid cdpOrganisationId,
        string userPrincipalId,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting membership for user {UserPrincipalId} in CDP organisation ID: {CdpOrganisationId}",
            userPrincipalId, cdpOrganisationId);

        var organisation = await organisationRepository.GetByCdpGuidAsync(cdpOrganisationId, cancellationToken)
                           ?? throw new EntityNotFoundException(nameof(Organisation), cdpOrganisationId);

        var membership =
            await membershipRepository.GetByUserAndOrganisationAsync(userPrincipalId, organisation.Id,
                cancellationToken);
        if (membership == null) return null;

        var assignments = await assignmentRepository.GetByMembershipIdAsync(membership.Id, cancellationToken);
        SetAssignments(membership, assignments);
        return membership;
    }

    public async Task<UserOrganisationMembership?> GetOrganisationUserByPersonIdAsync(
        Guid cdpOrganisationId,
        Guid cdpPersonId,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting membership for CDP person {CdpPersonId} in CDP organisation ID: {CdpOrganisationId}",
            cdpPersonId, cdpOrganisationId);

        var organisation = await organisationRepository.GetByCdpGuidAsync(cdpOrganisationId, cancellationToken)
                           ?? throw new EntityNotFoundException(nameof(Organisation), cdpOrganisationId);

        var membership =
            await membershipRepository.GetByPersonIdAndOrganisationAsync(cdpPersonId, organisation.Id,
                cancellationToken);
        if (membership == null) return null;

        var assignments = await assignmentRepository.GetByMembershipIdAsync(membership.Id, cancellationToken);
        SetAssignments(membership, assignments);
        return membership;
    }

    public async Task<UserOrganisationMembership> UpdateOrganisationRoleAsync(
        Guid cdpOrganisationId,
        Guid cdpPersonId,
        OrganisationRole organisationRole,
        CancellationToken cancellationToken = default)
    {
        await authorizationGuard.ValidateRoleChangeAsync(cdpOrganisationId, cdpPersonId, organisationRole,
            cancellationToken);
        var actingUserId = currentUserService.GetUserPrincipalId() ?? "unknown";
        return await updateRoleUseCase.Execute(
            new UpdateOrganisationRoleCommand(cdpOrganisationId, cdpPersonId, organisationRole, actingUserId),
            cancellationToken);
    }

    public async Task RemoveUserFromOrganisationAsync(
        Guid cdpOrganisationId,
        Guid cdpPersonId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await authorizationGuard.ValidateRemovalAsync(cdpOrganisationId, cdpPersonId, cancellationToken);
        }
        catch (EntityNotFoundException ex) when (ex.EntityName == nameof(UserOrganisationMembership))
        {
            // Membership already removed — operation is idempotent.
            return;
        }

        var actingUserId = currentUserService.GetUserPrincipalId() ?? "unknown";
        await removePersonUseCase.Execute(
            new RemovePersonFromOrganisationCommand(cdpOrganisationId, cdpPersonId, actingUserId),
            cancellationToken);
    }

    private static void SetAssignments(
        UserOrganisationMembership membership,
        IEnumerable<UserApplicationAssignment> assignments)
    {
        membership.ApplicationAssignments.Clear();
        foreach (var assignment in assignments)
            membership.ApplicationAssignments.Add(assignment);
    }
}