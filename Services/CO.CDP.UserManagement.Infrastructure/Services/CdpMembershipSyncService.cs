using CO.CDP.OrganisationInformation.Persistence.Constants;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CdpOrganisation = CO.CDP.OrganisationInformation.Persistence.Organisation;
using CdpPerson = CO.CDP.OrganisationInformation.Persistence.Person;
using IOrganisationRepository = CO.CDP.UserManagement.Core.Interfaces.IOrganisationRepository;
using OrganisationInformationContext = CO.CDP.OrganisationInformation.Persistence.OrganisationInformationContext;
using OrganisationPerson = CO.CDP.OrganisationInformation.Persistence.OrganisationPerson;
using UmOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Infrastructure.Services;

public class CdpMembershipSyncService(
    OrganisationInformationContext organisationInformationContext,
    IOrganisationRepository organisationRepository,
    IConfiguration configuration,
    ILogger<CdpMembershipSyncService> logger) : ICdpMembershipSyncService
{
    private const string FeatureFlagName = "Features:CdpMembershipSyncEnabled";

    public Task SyncMembershipCreatedAsync(UserOrganisationMembership membership, CancellationToken cancellationToken = default) =>
        SyncMembershipAsync(membership, MembershipSyncAction.Upsert, cancellationToken);

    public Task SyncMembershipRoleChangedAsync(UserOrganisationMembership membership, CancellationToken cancellationToken = default) =>
        SyncMembershipAsync(membership, MembershipSyncAction.Upsert, cancellationToken);

    public Task SyncMembershipRemovedAsync(UserOrganisationMembership membership, CancellationToken cancellationToken = default) =>
        SyncMembershipAsync(membership, MembershipSyncAction.Remove, cancellationToken);

    private async Task SyncMembershipAsync(
        UserOrganisationMembership membership,
        MembershipSyncAction action,
        CancellationToken cancellationToken)
    {
        var status = EvaluateSyncStatus(membership);
        var targetResult = await ResolveSyncTargetAsync(membership, status, cancellationToken);

        await ApplySyncAsync(targetResult, membership, action, cancellationToken);
    }

    private SyncStatus EvaluateSyncStatus(UserOrganisationMembership membership)
    {
        var featureEnabled = configuration.GetValue(FeatureFlagName, false);
        var hasCdpPersonId = membership.CdpPersonId.HasValue && membership.CdpPersonId != Guid.Empty;

        return (featureEnabled, hasCdpPersonId) switch
        {
            (false, _) => SyncStatus.FeatureDisabled,
            (true, false) => SyncStatus.MissingPersonId,
            _ => SyncStatus.Ready
        };
    }

    private async Task<SyncTargetResult> ResolveSyncTargetAsync(
        UserOrganisationMembership membership,
        SyncStatus status,
        CancellationToken cancellationToken)
    {
        var organisation = status == SyncStatus.Ready
            ? membership.Organisation
            : null;

        var cdpOrganisation = organisation == null
            ? null
            : await organisationInformationContext.Organisations
                .FirstOrDefaultAsync(o => o.Guid == organisation.CdpOrganisationGuid, cancellationToken);

        var cdpPerson = organisation == null || cdpOrganisation == null
            ? null
            : await organisationInformationContext.Persons
                .FirstOrDefaultAsync(p => p.Guid == membership.CdpPersonId, cancellationToken);

        var existingOrganisationPerson = organisation == null || cdpOrganisation == null || cdpPerson == null
            ? null
            : await organisationInformationContext.Set<OrganisationPerson>()
                .FirstOrDefaultAsync(
                    op => op.OrganisationId == cdpOrganisation.Id && op.PersonId == cdpPerson.Id,
                    cancellationToken);

        var resolvedStatus = status == SyncStatus.Ready
            ? DetermineDataStatus(organisation, cdpOrganisation, cdpPerson)
            : status;

        return new SyncTargetResult(
            resolvedStatus,
            organisation,
            cdpOrganisation,
            cdpPerson,
            existingOrganisationPerson);
    }

    private static SyncStatus DetermineDataStatus(
        UmOrganisation? organisation,
        CdpOrganisation? cdpOrganisation,
        CdpPerson? cdpPerson)
    {
        return (organisation, cdpOrganisation, cdpPerson) switch
        {
            (null, _, _) => SyncStatus.OrganisationMissing,
            (_, null, _) => SyncStatus.CdpOrganisationMissing,
            (_, _, null) => SyncStatus.CdpPersonMissing,
            _ => SyncStatus.Ready
        };
    }

    private async Task ApplySyncAsync(
        SyncTargetResult targetResult,
        UserOrganisationMembership membership,
        MembershipSyncAction action,
        CancellationToken cancellationToken)
    {
        switch (targetResult.Status)
        {
            case SyncStatus.FeatureDisabled:
                logger.LogDebug("CDP membership sync is disabled for membership {MembershipId}", membership.Id);
                break;
            case SyncStatus.MissingPersonId:
                logger.LogWarning("Skipping CDP membership sync for membership {MembershipId} because CdpPersonId is missing",
                    membership.Id);
                break;
            case SyncStatus.OrganisationMissing:
                logger.LogError("Organisation {OrganisationId} not found when syncing membership {MembershipId}",
                    membership.OrganisationId, membership.Id);
                break;
            case SyncStatus.CdpOrganisationMissing:
                logger.LogError("CDP organisation {CdpOrganisationGuid} not found when syncing membership {MembershipId}",
                    targetResult.Organisation?.CdpOrganisationGuid, membership.Id);
                break;
            case SyncStatus.CdpPersonMissing:
                logger.LogError("CDP person {CdpPersonId} not found when syncing membership {MembershipId}",
                    membership.CdpPersonId, membership.Id);
                break;
            case SyncStatus.Ready:
                await SyncOrganisationPersonAsync(targetResult, membership, action, cancellationToken);
                break;
        }
    }

    private async Task SyncOrganisationPersonAsync(
        SyncTargetResult targetResult,
        UserOrganisationMembership membership,
        MembershipSyncAction action,
        CancellationToken cancellationToken)
    {
        var cdpOrganisation = targetResult.CdpOrganisation!;
        var cdpPerson = targetResult.CdpPerson!;
        var existingOrganisationPerson = targetResult.ExistingOrganisationPerson;

        if (action == MembershipSyncAction.Remove)
        {
            if (existingOrganisationPerson != null)
            {
                organisationInformationContext.Remove(existingOrganisationPerson);
                await SaveChangesAsync(membership.Id, action, cancellationToken);
            }
            else
            {
                logger.LogInformation(
                    "OrganisationPerson not found for membership {MembershipId}, skipping removal",
                    membership.Id);
            }
        }
        else
        {
            var scopes = MapOrganisationRoleToScopes(membership.OrganisationRole);
            var organisationPerson = existingOrganisationPerson ?? new OrganisationPerson
            {
                OrganisationId = cdpOrganisation.Id,
                Organisation = cdpOrganisation,
                PersonId = cdpPerson.Id,
                Person = cdpPerson,
                Scopes = scopes
            };

            if (existingOrganisationPerson == null)
            {
                organisationInformationContext.Add(organisationPerson);
            }
            else
            {
                organisationPerson.Scopes = scopes;
            }

            await SaveChangesAsync(membership.Id, action, cancellationToken);
        }
    }

    private async Task SaveChangesAsync(int membershipId, MembershipSyncAction action, CancellationToken cancellationToken)
    {
        try
        {
            await organisationInformationContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Synced membership {MembershipId} to CDP with action {Action}", membershipId, action);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to sync membership {MembershipId} to CDP with action {Action}", membershipId, action);
        }
    }

    private static List<string> MapOrganisationRoleToScopes(OrganisationRole organisationRole)
    {
        return organisationRole switch
        {
            OrganisationRole.Owner => new List<string> { OrganisationPersonScopes.Admin },
            OrganisationRole.Admin => new List<string> { OrganisationPersonScopes.Admin },
            _ => new List<string> { OrganisationPersonScopes.Viewer }
        };
    }

    private enum MembershipSyncAction
    {
        Upsert,
        Remove
    }

    private enum SyncStatus
    {
        Ready,
        FeatureDisabled,
        MissingPersonId,
        OrganisationMissing,
        CdpOrganisationMissing,
        CdpPersonMissing
    }

    private sealed record SyncTargetResult(
        SyncStatus Status,
        UmOrganisation? Organisation,
        CdpOrganisation? CdpOrganisation,
        CdpPerson? CdpPerson,
        OrganisationPerson? ExistingOrganisationPerson);
}
