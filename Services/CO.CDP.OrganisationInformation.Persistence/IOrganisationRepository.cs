namespace CO.CDP.OrganisationInformation.Persistence;

public interface IOrganisationRepository : IDisposable
{
    public void Save(Organisation organisation);

    public void SaveOrganisationPerson(OrganisationPerson organisationPerson);    

    public Task<Organisation?> Find(Guid organisationId);

    public Task<OrganisationPerson?> FindOrganisationPerson(Guid organisationId, Guid personId);

    public Task<Organisation?> FindByName(string name);

    public Task<IEnumerable<Organisation>> FindByUserUrn(string userUrn);

    public Task<Organisation?> FindByIdentifier(string scheme, string identifierId);

    public class OrganisationRepositoryException(string message, Exception? cause = null) : Exception(message, cause)
    {
        public class DuplicateOrganisationException(string message, Exception? cause = null)
            : OrganisationRepositoryException(message, cause);
    }

    public Task<IList<ConnectedEntity>> GetConnectedIndividualTrusts(int organisationId);

    public Task<IList<ConnectedEntity>> GetConnectedOrganisations(int organisationId);
}