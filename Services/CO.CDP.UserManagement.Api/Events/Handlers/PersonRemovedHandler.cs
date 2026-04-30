using CO.CDP.MQ;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Events;

namespace CO.CDP.UserManagement.Api.Events.Handlers;

public class PersonRemovedHandler(
    IUmOrganisationSyncRepository syncRepo,
    IUnitOfWork unitOfWork,
    IClaimsCacheService claimsCacheService,
    IOrganisationRepository organisationRepository,
    IUserOrganisationMembershipRepository membershipRepository,
    ILogger<PersonRemovedHandler> logger) : ISubscriber<PersonRemovedFromOrganisation>
{
    public async Task Handle(PersonRemovedFromOrganisation @event)
    {
        var orgGuid = Guid.Parse(@event.OrganisationId);
        var personGuid = Guid.Parse(@event.PersonId);

        logger.LogInformation(
            "[PersonRemovedHandler] Processing removal of person {PersonGuid} from org {OrgGuid}",
            personGuid, orgGuid);

        var userPrincipalId = await ResolveUserPrincipalIdAsync(orgGuid, personGuid);

        (await syncRepo.EnsureMemberRemovedAsync(orgGuid, personGuid))
            .ThrowOnFailure("EnsureMemberRemovedAsync", logger);

        await unitOfWork.SaveChangesAsync();

        if (userPrincipalId != null)
            await InvalidateCacheAsync(userPrincipalId);

        logger.LogInformation(
            "[PersonRemovedHandler] Completed removal of person {PersonGuid} from org {OrgGuid}",
            personGuid, orgGuid);
    }

    private async Task<string?> ResolveUserPrincipalIdAsync(Guid orgGuid, Guid personGuid)
    {
        try
        {
            var org = await organisationRepository.GetByCdpGuidAsync(orgGuid);
            if (org is null) return null;
            var membership = await membershipRepository.GetByPersonIdAndOrganisationAsync(personGuid, org.Id);
            return membership?.UserPrincipalId;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "[PersonRemovedHandler] Failed to resolve UserPrincipalId for person {PersonGuid} in org {OrgGuid}",
                personGuid, orgGuid);
            return null;
        }
    }

    private async Task InvalidateCacheAsync(string userPrincipalId)
    {
        try
        {
            await claimsCacheService.InvalidateCacheAsync(userPrincipalId);
            logger.LogInformation(
                "[PersonRemovedHandler] Invalidated claims cache for {UserPrincipalId}", userPrincipalId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "[PersonRemovedHandler] Failed to invalidate claims cache for {UserPrincipalId}", userPrincipalId);
        }
    }
}