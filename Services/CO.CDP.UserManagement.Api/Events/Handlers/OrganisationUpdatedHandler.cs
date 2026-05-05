using CO.CDP.MQ;
using CO.CDP.UserManagement.Core.Interfaces;

namespace CO.CDP.UserManagement.Api.Events.Handlers;

public class OrganisationUpdatedHandler(
    IUmOrganisationSyncRepository syncRepo,
    IUnitOfWork unitOfWork,
    ILogger<OrganisationUpdatedHandler> logger) : ISubscriber<OrganisationUpdated>
{
    public async Task Handle(OrganisationUpdated @event)
    {
        var orgGuid = Guid.Parse(@event.Id);

        logger.LogInformation(
            "[OrganisationUpdatedHandler] Processing name sync for org {OrgGuid}, name={Name}",
            orgGuid, @event.Name);

        (await syncRepo.EnsureNameSyncedAsync(orgGuid, @event.Name))
            .ThrowOnFailure("EnsureNameSyncedAsync", logger);

        await unitOfWork.SaveChangesAsync();
        logger.LogInformation("[OrganisationUpdatedHandler] Completed name sync for org {OrgGuid}", orgGuid);
    }
}