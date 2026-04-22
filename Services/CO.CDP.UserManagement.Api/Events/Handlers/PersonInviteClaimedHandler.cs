using CO.CDP.MQ;
using CO.CDP.UserManagement.Core.Interfaces;

namespace CO.CDP.UserManagement.Api.Events.Handlers;

public class PersonInviteClaimedHandler(
    IUmOrganisationSyncRepository syncRepo,
    IUnitOfWork unitOfWork,
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
        logger.LogInformation(
            "[PersonInviteClaimedHandler] Completed invite claim for person {PersonGuid} in org {OrgGuid}",
            personGuid, orgGuid);
    }
}