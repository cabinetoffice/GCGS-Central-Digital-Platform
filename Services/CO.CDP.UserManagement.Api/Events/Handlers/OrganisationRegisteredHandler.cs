using CO.CDP.MQ;
using CO.CDP.UserManagement.Core.Interfaces;

namespace CO.CDP.UserManagement.Api.Events.Handlers;

public class OrganisationRegisteredHandler(
    IUmOrganisationSyncRepository syncRepo,
    IUnitOfWork unitOfWork,
    IClaimsCacheService claimsCacheService,
    ILogger<OrganisationRegisteredHandler> logger) : ISubscriber<OrganisationRegistered>
{
    public async Task Handle(OrganisationRegistered @event)
    {
        var orgGuid = Guid.Parse(@event.Id);

        logger.LogInformation(
            "[OrganisationRegisteredHandler] Processing OrganisationRegistered for org {OrgGuid}, name={Name}",
            orgGuid, @event.Name);

        (await syncRepo.EnsureCreatedAsync(orgGuid, @event.Name))
            .ThrowOnFailure("EnsureCreatedAsync", logger);

        await unitOfWork.SaveChangesAsync();
        logger.LogInformation("[OrganisationRegisteredHandler] Organisation created/verified for {OrgGuid}", orgGuid);

        (await syncRepo.EnsureActiveApplicationsEnabledAsync(orgGuid))
            .ThrowOnFailure("EnsureActiveApplicationsEnabledAsync", logger);

        await unitOfWork.SaveChangesAsync();
        logger.LogInformation("[OrganisationRegisteredHandler] Applications enabled for {OrgGuid}", orgGuid);

        if (@event.FounderPersonId.HasValue && !string.IsNullOrEmpty(@event.FounderUserUrn))
        {
            (await syncRepo.EnsureFounderOwnerCreatedAsync(
                    orgGuid, @event.FounderPersonId.Value, @event.FounderUserUrn))
                .ThrowOnFailure("EnsureFounderOwnerCreatedAsync", logger);

            await unitOfWork.SaveChangesAsync();

            await InvalidateCacheAsync(@event.FounderUserUrn);

            logger.LogInformation(
                "[OrganisationRegisteredHandler] Founder owner created for org {OrgGuid}, person {PersonGuid}",
                orgGuid, @event.FounderPersonId.Value);
        }
        else
        {
            await unitOfWork.SaveChangesAsync();
            logger.LogInformation(
                "[OrganisationRegisteredHandler] No founder info — skipping owner creation for {OrgGuid}", orgGuid);
        }

        logger.LogInformation("[OrganisationRegisteredHandler] Completed OrganisationRegistered for org {OrgGuid}",
            orgGuid);
    }

    private async Task InvalidateCacheAsync(string userPrincipalId)
    {
        try
        {
            await claimsCacheService.InvalidateCacheAsync(userPrincipalId);
            logger.LogInformation(
                "[OrganisationRegisteredHandler] Invalidated claims cache for {UserPrincipalId}", userPrincipalId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "[OrganisationRegisteredHandler] Failed to invalidate claims cache for {UserPrincipalId}",
                userPrincipalId);
        }
    }
}