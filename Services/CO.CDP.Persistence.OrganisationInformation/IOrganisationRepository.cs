namespace CO.CDP.Persistence.OrganisationInformation;

public interface IOrganisationRepository : IDisposable
{
    public void Save(Organisation organisation);

    public Task<Organisation?> Find(Guid organisationId);

    public Task<Organisation?> FindByName(string name);

    public class OrganisationRepositoryException(string message, Exception? cause = null) : Exception(message, cause)
    {
        public class DuplicateOrganisationException(string message, Exception? cause = null)
            : OrganisationRepositoryException(message, cause);
    }
}