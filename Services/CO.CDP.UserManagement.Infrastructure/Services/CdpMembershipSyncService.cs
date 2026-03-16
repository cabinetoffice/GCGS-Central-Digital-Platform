using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CO.CDP.UserManagement.Infrastructure.Services;

public class CdpMembershipSyncService(
    IUserOrganisationMembershipRepository membershipRepository,
    IRoleMappingService roleMappingService,
    IOrganisationPersonSyncRepository organisationPersonSyncRepository,
    ILogger<CdpMembershipSyncService> logger) : ICdpMembershipSyncService
{
    public Task SyncMembershipCreatedAsync(UserOrganisationMembership membership, CancellationToken cancellationToken = default) =>
        SyncMembershipAccessChangedAsync(membership.Id, cancellationToken);

    public Task SyncMembershipRoleChangedAsync(UserOrganisationMembership membership, CancellationToken cancellationToken = default) =>
        SyncMembershipAccessChangedAsync(membership.Id, cancellationToken);

    public async Task SyncMembershipRemovedAsync(UserOrganisationMembership membership, CancellationToken cancellationToken = default)
    {
        var org = membership.Organisation
            ?? (await membershipRepository.GetWithOrganisationAndRoleAsync(membership.Id, cancellationToken))?.Organisation;

        if (org == null || !membership.CdpPersonId.HasValue || membership.CdpPersonId == Guid.Empty)
        {
            logger.LogWarning(
                "Skipping removal sync for membership {MembershipId}: missing organisation or CdpPersonId",
                membership.Id);
            return;
        }

        await organisationPersonSyncRepository.RemoveAsync(
            org.CdpOrganisationGuid,
            membership.CdpPersonId.Value,
            cancellationToken);

        logger.LogInformation(
            "Removed OrganisationPerson for membership {MembershipId}", membership.Id);
    }

    public async Task SyncMembershipAccessChangedAsync(int membershipId, CancellationToken cancellationToken = default)
    {
        var membership = await membershipRepository.GetWithOrganisationAndRoleAsync(membershipId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(UserOrganisationMembership), membershipId);

        if (!membership.IsActive || !membership.CdpPersonId.HasValue || membership.CdpPersonId == Guid.Empty)
        {
            logger.LogInformation(
                "Skipping OI sync for membership {MembershipId}: inactive or missing CdpPersonId", membershipId);
            return;
        }

        var shouldSync = await roleMappingService.ShouldSyncToOrganisationInformationAsync(membershipId, cancellationToken);
        if (!shouldSync)
        {
            logger.LogInformation(
                "Skipping OI sync for membership {MembershipId}: role is not sync-enabled", membershipId);
            return;
        }

        var scopes = await roleMappingService.GetOrganisationInformationScopesAsync(membershipId, cancellationToken);

        await organisationPersonSyncRepository.UpsertAsync(
            membership.Organisation.CdpOrganisationGuid,
            membership.CdpPersonId.Value,
            scopes,
            cancellationToken);

        logger.LogInformation(
            "Synced OrganisationPerson for membership {MembershipId} with {ScopeCount} scope(s)",
            membershipId, scopes.Count);
    }
}
