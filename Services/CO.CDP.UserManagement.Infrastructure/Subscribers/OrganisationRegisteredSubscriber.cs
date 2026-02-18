using CO.CDP.MQ;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Events;

namespace CO.CDP.UserManagement.Infrastructure.Subscribers;

/// <summary>
/// Handles OrganisationRegistered events from CDP to auto-sync organisations into UserManagement.
/// </summary>
public class OrganisationRegisteredSubscriber(
    IOrganisationSyncService organisationSyncService)
    : ISubscriber<OrganisationRegistered>
{
    public async Task Handle(OrganisationRegistered @event)
    {
        await organisationSyncService.SyncRegisteredAsync(@event.Id, @event.Name);
    }
}
