using CO.CDP.MQ;
using CO.CDP.UserManagement.Core.Interfaces;

namespace CO.CDP.UserManagement.Api.Events.Handlers;

public class PersonInviteClaimedHandler(
    IUmOrganisationSyncRepository syncRepo,
    IUnitOfWork unitOfWork,
    IClaimsCacheService claimsCacheService,
    ILogger<PersonInviteClaimedHandler> logger) : ISubscriber<PersonInviteClaimed>
{
    public async Task Handle(PersonInviteClaimed @event)
    {
        var orgGuid = Guid.Parse(@event.OrganisationId);
        var personGuid = Guid.Parse(@event.PersonId);

        logger.LogInformation(
            "[PersonInviteClaimedHandler] Processing invite claim for person {PersonGuid} in org {OrgGuid}, scopes=[{Scopes}]",
            personGuid, orgGuid, string.Join(",", @event.Scopes));

        (await syncRepo.EnsureMemberCreatedAsync(
                orgGuid, personGuid, @event.UserPrincipalId, @event.Scopes))
            .ThrowOnFailure("EnsureMemberCreatedAsync", logger);

        await unitOfWork.SaveChangesAsync();

        await InvalidateCacheAsync(@event.UserPrincipalId);

        logger.LogInformation(
            "[PersonInviteClaimedHandler] Completed invite claim for person {PersonGuid} in org {OrgGuid}",
            personGuid, orgGuid);
    }

    private async Task InvalidateCacheAsync(string userPrincipalId)
    {
        try
        {
            await claimsCacheService.InvalidateCacheAsync(userPrincipalId);
            logger.LogInformation(
                "[PersonInviteClaimedHandler] Invalidated claims cache for {UserPrincipalId}", userPrincipalId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "[PersonInviteClaimedHandler] Failed to invalidate claims cache for {UserPrincipalId}",
                userPrincipalId);
        }
    }
}