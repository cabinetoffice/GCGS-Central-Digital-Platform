using CO.CDP.Functional;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace CO.CDP.OrganisationSync;

/// <summary>
/// Atomic bidirectional sync between UM and OI databases.
/// All writes are wrapped in <see cref="IAtomicScope.ExecuteAsync"/> so both
/// <c>OrganisationInformationContext</c> and <c>UserManagementDbContext</c> commit
/// or roll back together.
/// </summary>
public sealed class AtomicMembershipSync(
    IAtomicScope atomicScope,
    IOrganisationRepository organisationRepository,
    IUserOrganisationMembershipRepository membershipRepository,
    IUserApplicationAssignmentRepository assignmentRepository,
    IRoleMappingService roleMappingService,
    IOrganisationPersonSyncRepository organisationPersonSyncRepository,
    ICurrentUserService currentUserService,
    ILogger<AtomicMembershipSync> logger) : IAtomicMembershipSync
{
    public Task RemoveUserFromOrganisationAsync(
        Guid cdpOrganisationId,
        Guid cdpPersonId,
        CancellationToken ct = default) =>
        atomicScope.ExecuteAsync(token => RemoveAsync(cdpOrganisationId, cdpPersonId, token), ct);

    public Task<UserOrganisationMembership> UpdateMembershipRoleAsync(
        Guid cdpOrganisationId,
        Guid cdpPersonId,
        OrganisationRole newRole,
        CancellationToken ct = default) =>
        atomicScope.ExecuteAsync(token => UpdateRoleAsync(cdpOrganisationId, cdpPersonId, newRole, token), ct);

    private async Task<Unit> RemoveAsync(
        Guid cdpOrganisationId,
        Guid cdpPersonId,
        CancellationToken ct)
    {
        var organisation = await organisationRepository.GetByCdpGuidAsync(cdpOrganisationId, ct)
            ?? throw new EntityNotFoundException(nameof(Organisation), cdpOrganisationId);

        var membership = await membershipRepository.GetByPersonIdAndOrganisationAsync(cdpPersonId, organisation.Id, ct)
            ?? throw new EntityNotFoundException(nameof(UserOrganisationMembership), cdpPersonId);

        if (!membership.IsActive)
            return Unit.Value;

        if (membership.OrganisationRole == OrganisationRole.Owner)
        {
            var ownerCount = await membershipRepository.CountActiveOwnersByOrganisationIdAsync(organisation.Id, ct);
            if (ownerCount <= 1)
                throw new LastOwnerRemovalException(cdpOrganisationId);
        }

        var now = DateTimeOffset.UtcNow;
        var actingUser = currentUserService.GetUserPrincipalId() ?? "unknown";

        membership.IsActive = false;
        membership.IsDeleted = true;
        membership.DeletedAt = now;
        membership.DeletedBy = actingUser;
        membershipRepository.Update(membership);

        var assignments = await assignmentRepository.GetByMembershipIdAsync(membership.Id, ct);
        foreach (var assignment in assignments.Where(a => a.IsActive))
        {
            assignment.IsActive = false;
            assignment.RevokedAt = now;
            assignment.RevokedBy = actingUser;
            assignmentRepository.Update(assignment);
        }

        if (membership.CdpPersonId.HasValue)
            await organisationPersonSyncRepository.RemoveAsync(organisation.CdpOrganisationGuid, membership.CdpPersonId.Value, ct);

        logger.LogInformation(
            "User {CdpPersonId} removed from organisation {CdpOrganisationId} by {ActingUser}",
            cdpPersonId, cdpOrganisationId, actingUser);

        return Unit.Value;
    }

    private async Task<UserOrganisationMembership> UpdateRoleAsync(
        Guid cdpOrganisationId,
        Guid cdpPersonId,
        OrganisationRole newRole,
        CancellationToken ct)
    {
        var organisation = await organisationRepository.GetByCdpGuidAsync(cdpOrganisationId, ct)
            ?? throw new EntityNotFoundException(nameof(Organisation), cdpOrganisationId);

        var membership = await membershipRepository.GetByPersonIdAndOrganisationAsync(cdpPersonId, organisation.Id, ct)
            ?? throw new EntityNotFoundException(nameof(UserOrganisationMembership), cdpPersonId);

        await roleMappingService.ApplyRoleDefinitionAsync(membership, newRole, ct);
        membershipRepository.Update(membership);

        var shouldSync = await roleMappingService.ShouldSyncToOrganisationInformationAsync(membership.Id, ct);
        if (shouldSync && membership.CdpPersonId.HasValue)
        {
            var scopes = await roleMappingService.GetOrganisationInformationScopesAsync(membership.Id, ct);
            await organisationPersonSyncRepository.UpsertAsync(organisation.CdpOrganisationGuid, membership.CdpPersonId.Value, scopes, ct);
        }

        logger.LogInformation(
            "Role for user {CdpPersonId} in organisation {CdpOrganisationId} updated to {NewRole}",
            cdpPersonId, cdpOrganisationId, newRole);

        return membership;
    }
}
