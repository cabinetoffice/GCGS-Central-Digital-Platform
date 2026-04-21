using CO.CDP.MQ;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Events;

namespace CO.CDP.UserManagement.Api.Events.Handlers;

public class PersonRemovedHandler(
    IUmOrganisationSyncRepository syncRepo,
    IUnitOfWork unitOfWork,
    ILogger<PersonRemovedHandler> logger) : ISubscriber<PersonRemovedFromOrganisation>
{
    public async Task Handle(PersonRemovedFromOrganisation @event)
    {
        var orgGuid = Guid.Parse(@event.OrganisationId);
        var personGuid = Guid.Parse(@event.PersonId);

        logger.LogInformation(
            "[PersonRemovedHandler] Processing removal of person {PersonGuid} from org {OrgGuid}",
            personGuid, orgGuid);

        (await syncRepo.EnsureMemberRemovedAsync(orgGuid, personGuid))
            .ThrowOnFailure("EnsureMemberRemovedAsync", logger);

        await unitOfWork.SaveChangesAsync();
        logger.LogInformation(
            "[PersonRemovedHandler] Completed removal of person {PersonGuid} from org {OrgGuid}",
            personGuid, orgGuid);
    }
}