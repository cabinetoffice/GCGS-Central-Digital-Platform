using CO.CDP.MQ;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Events;

namespace CO.CDP.UserManagement.Api.Events.Handlers;

public class PersonScopesUpdatedHandler(
    IUmOrganisationSyncRepository syncRepo,
    IUnitOfWork unitOfWork,
    ILogger<PersonScopesUpdatedHandler> logger) : ISubscriber<PersonScopesUpdated>
{
    public async Task Handle(PersonScopesUpdated @event)
    {
        var orgGuid = Guid.Parse(@event.OrganisationId);
        var personGuid = Guid.Parse(@event.PersonId);

        logger.LogInformation(
            "[PersonScopesUpdatedHandler] Processing scope update for person {PersonGuid} in org {OrgGuid}, scopes=[{Scopes}]",
            personGuid, orgGuid, string.Join(",", @event.Scopes));

        (await syncRepo.EnsureMemberScopesAndAppRolesUpdatedAsync(
                orgGuid, personGuid, @event.Scopes))
            .ThrowOnFailure("EnsureMemberScopesAndAppRolesUpdatedAsync", logger);

        await unitOfWork.SaveChangesAsync();
        logger.LogInformation(
            "[PersonScopesUpdatedHandler] Completed scope update for person {PersonGuid} in org {OrgGuid}",
            personGuid, orgGuid);
    }
}