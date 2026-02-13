namespace CO.CDP.UserManagement.Core.Interfaces;

public interface IOrganisationSyncService
{
    Task SyncRegisteredAsync(string id, string name, CancellationToken cancellationToken = default);

    Task SyncUpdatedAsync(string id, string name, CancellationToken cancellationToken = default);
}
