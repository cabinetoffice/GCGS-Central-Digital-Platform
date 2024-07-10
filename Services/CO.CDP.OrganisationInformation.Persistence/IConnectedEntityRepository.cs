namespace CO.CDP.OrganisationInformation.Persistence;

public interface IConnectedEntityRepository : IDisposable
{
    public Task Save(ConnectedEntity connectedEntity);

    public Task<ConnectedEntity?> Find(Guid id);

    public Task<IEnumerable<ConnectedEntity?>> FindByOrganisation(Guid organisationId);

    public Task<IEnumerable<ConnectedEntityLookup?>> GetSummary(Guid organisationId);

    public class ConnectedEntityRepositoryException(string message, Exception? cause = null) : Exception(message, cause)
    {
        public class DuplicateConnectedEntityException(string message, Exception? cause = null)
            : ConnectedEntityRepositoryException(message, cause);
    }
}