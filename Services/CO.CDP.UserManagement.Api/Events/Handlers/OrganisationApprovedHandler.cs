using CO.CDP.MQ;
using CO.CDP.UserManagement.Core.Interfaces;

namespace CO.CDP.UserManagement.Api.Events.Handlers;

public class OrganisationApprovedHandler(
    IUmOrganisationSyncRepository syncRepo,
    IUnitOfWork unitOfWork,
    ILogger<OrganisationApprovedHandler> logger) : ISubscriber<OrganisationApproved>
{
    public async Task Handle(OrganisationApproved @event)
    {
        var orgGuid = Guid.Parse(@event.Id);

        logger.LogInformation(
            "[OrganisationApprovedHandler] Processing OrganisationApproved for org {OrgGuid}", orgGuid);

        (await syncRepo.EnsureOrganisationDefaultsReappliedAsync(orgGuid))
            .ThrowOnFailure("EnsureOrganisationDefaultsReappliedAsync", logger);

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation(
            "[OrganisationApprovedHandler] Completed default role resync for org {OrgGuid}", orgGuid);
    }
}