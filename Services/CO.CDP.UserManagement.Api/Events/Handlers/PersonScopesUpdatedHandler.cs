using CO.CDP.MQ;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Events;

namespace CO.CDP.UserManagement.Api.Events.Handlers;

public class PersonScopesUpdatedHandler(
    IUmOrganisationSyncRepository syncRepo,
    IUnitOfWork unitOfWork,
    IClaimsCacheService claimsCacheService,
    IOrganisationRepository organisationRepository,
    IUserOrganisationMembershipRepository membershipRepository,
    ILogger<PersonScopesUpdatedHandler> logger) : ISubscriber<PersonScopesUpdated>
{
    public async Task Handle(PersonScopesUpdated @event)
    {
        var orgGuid = Guid.Parse(@event.OrganisationId);
        var personGuid = Guid.Parse(@event.PersonId);

        logger.LogInformation(
            "[PersonScopesUpdatedHandler] Processing scope update for person {PersonGuid} in org {OrgGuid}, scopes=[{Scopes}]",
            personGuid, orgGuid, string.Join(",", @event.Scopes));

        var userPrincipalId = await ResolveUserPrincipalIdAsync(orgGuid, personGuid);

        (await syncRepo.EnsureMemberScopesAndAppRolesUpdatedAsync(
                orgGuid, personGuid, @event.Scopes))
            .ThrowOnFailure("EnsureMemberScopesAndAppRolesUpdatedAsync", logger);

        await unitOfWork.SaveChangesAsync();

        if (userPrincipalId != null)
            await InvalidateCacheAsync(userPrincipalId);

        logger.LogInformation(
            "[PersonScopesUpdatedHandler] Completed scope update for person {PersonGuid} in org {OrgGuid}",
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
                "[PersonScopesUpdatedHandler] Failed to resolve UserPrincipalId for person {PersonGuid} in org {OrgGuid}",
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
                "[PersonScopesUpdatedHandler] Invalidated claims cache for {UserPrincipalId}", userPrincipalId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "[PersonScopesUpdatedHandler] Failed to invalidate claims cache for {UserPrincipalId}",
                userPrincipalId);
        }
    }
}