using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Constants;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace CO.CDP.UserManagement.Infrastructure.Services;

public class CdpMembershipSyncService(
    IUserOrganisationMembershipRepository membershipRepository,
    IRoleMappingService roleMappingService,
    IOrganisationPersonSyncRepository organisationPersonSyncRepository,
    IFeatureManager featureManager,
    ILogger<CdpMembershipSyncService> logger) : ICdpMembershipSyncService
{
    public Task SyncMembershipCreatedAsync(UserOrganisationMembership membership, CancellationToken cancellationToken = default) =>
        SyncMembershipAccessChangedAsync(membership.Id, cancellationToken);

    public Task SyncMembershipRoleChangedAsync(UserOrganisationMembership membership, CancellationToken cancellationToken = default) =>
        SyncMembershipAccessChangedAsync(membership.Id, cancellationToken);

    public async Task SyncMembershipRemovedAsync(UserOrganisationMembership membership, CancellationToken cancellationToken = default)
    {
        if (!await featureManager.IsEnabledAsync(FeatureFlags.OrganisationSyncEnabled))
        {
            logger.LogInformation(
                "Skipping removal sync for membership {MembershipId}: OrganisationSyncEnabled is disabled",
                membership.Id);
            return;
        }

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
        if (!await featureManager.IsEnabledAsync(FeatureFlags.OrganisationSyncEnabled))
        {
            logger.LogInformation(
                "Skipping OI sync for membership {MembershipId}: OrganisationSyncEnabled is disabled", membershipId);
            return;
        }

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
