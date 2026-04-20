using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.OrganisationRoles;
using CO.CDP.UserManagement.Core.Removal;
using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.Infrastructure.Services;

/// <summary>
/// Pre-flight authorization guard for membership mutation operations.
/// Resolves both the actor's and the target's current organisation role from the
/// repository (never from JWT claims, which may be stale) before applying the
/// policy decisions in <see cref="UserRemovalValidator"/> and
/// <see cref="OrganisationRoleChangeValidator"/>.
///
/// Note: last-owner protection is a transactional invariant and is enforced
/// separately inside <c>AtomicMembershipSync</c>.
/// </summary>
public class MembershipAuthorizationGuard(
    ICurrentUserService currentUserService,
    IOrganisationRepository organisationRepository,
    IUserOrganisationMembershipRepository membershipRepository) : IMembershipAuthorizationGuard
{
    public async Task ValidateRemovalAsync(
        Guid cdpOrganisationId, Guid cdpPersonId, CancellationToken ct = default)
    {
        var (actorPrincipalId, actorRole, targetMembership) =
            await ResolveContextAsync(cdpOrganisationId, cdpPersonId, ct);

        var result = UserRemovalValidator.Validate(
            targetEmail: targetMembership.UserPrincipalId,
            currentUserEmail: actorPrincipalId,
            targetOrganisationRole: targetMembership.OrganisationRole,
            isLastOwner: false, // transactional invariant — checked inside AtomicMembershipSync
            currentUserOrganisationRole: actorRole);

        if (!result.IsValid)
            throw new MembershipOperationForbiddenException(result.ErrorMessage!);
    }

    public async Task ValidateRoleChangeAsync(
        Guid cdpOrganisationId, Guid cdpPersonId, OrganisationRole newRole, CancellationToken ct = default)
    {
        var (actorPrincipalId, actorRole, targetMembership) =
            await ResolveContextAsync(cdpOrganisationId, cdpPersonId, ct);

        var result = OrganisationRoleChangeValidator.Validate(
            selectedRole: newRole,
            currentRole: targetMembership.OrganisationRole,
            targetEmail: targetMembership.UserPrincipalId,
            currentUserEmail: actorPrincipalId,
            currentUserOrganisationRole: actorRole);

        if (!result.IsValid)
            throw new MembershipOperationForbiddenException(result.ErrorMessage!);
    }

    /// <summary>
    /// Resolves the actor's principal ID and their live organisation role, plus the
    /// target membership, all from the repository.
    /// </summary>
    private async Task<(string actorPrincipalId, OrganisationRole? actorRole, Core.Entities.UserOrganisationMembership targetMembership)>
        ResolveContextAsync(Guid cdpOrganisationId, Guid cdpPersonId, CancellationToken ct)
    {
        var actorPrincipalId = currentUserService.GetUserPrincipalId()
            ?? throw new MembershipOperationForbiddenException("Unable to determine the current user.");

        var organisation = await organisationRepository.GetByCdpGuidAsync(cdpOrganisationId, ct)
            ?? throw new EntityNotFoundException(
                nameof(Core.Entities.Organisation), cdpOrganisationId);

        var actorMembership = await membershipRepository.GetByUserAndOrganisationAsync(
            actorPrincipalId, organisation.Id, ct);
        var actorRole = actorMembership?.OrganisationRole;

        var targetMembership = await membershipRepository.GetByPersonIdAndOrganisationAsync(
            cdpPersonId, organisation.Id, ct)
            ?? throw new EntityNotFoundException(
                nameof(Core.Entities.UserOrganisationMembership), cdpPersonId);

        return (actorPrincipalId, actorRole, targetMembership);
    }
}
