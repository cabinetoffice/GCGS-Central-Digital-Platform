namespace CO.CDP.OrganisationInformation.Persistence;

public interface IConnectedEntityRepository : IDisposable
{
    public Task Save(ConnectedEntity connectedEntity);

    public Task<ConnectedEntity?> Find(Guid organisationId, Guid id);

    public Task<IEnumerable<ConnectedEntityLookup?>> GetSummary(Guid organisationId);

    public Task<Tuple<bool, Guid, Guid>> IsConnectedEntityUsedInExclusionAsync(Guid organisationId, Guid connectedEntityId);

    public class ConnectedEntityRepositoryException(string message, Exception? cause = null) : Exception(message, cause)
    {
        public class DuplicateConnectedEntityException(string message, Exception? cause = null)
            : ConnectedEntityRepositoryException(message, cause);
    }
}