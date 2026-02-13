using CO.CDP.MQ;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Events;

namespace CO.CDP.UserManagement.Infrastructure.Subscribers;

/// <summary>
/// Handles OrganisationUpdated events from CDP to keep UserManagement organisations in sync.
/// </summary>
public class OrganisationUpdatedSubscriber(
    IOrganisationSyncService organisationSyncService)
    : ISubscriber<OrganisationUpdated>
{
    public async Task Handle(OrganisationUpdated @event)
    {
        await organisationSyncService.SyncUpdatedAsync(@event.Id, @event.Name);
    }
}
