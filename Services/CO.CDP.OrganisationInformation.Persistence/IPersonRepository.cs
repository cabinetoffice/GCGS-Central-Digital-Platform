namespace CO.CDP.OrganisationInformation.Persistence;

public interface IPersonRepository : IDisposable
{
    public void Save(Person person);

    public Task<Person?> Find(Guid personId); 

    public Task<Person?> FindByUrn(string urn);

    public Task<IEnumerable<Person>> FindByOrganisation(Guid organisationId);

    public class PersonRepositoryException(string message, Exception? cause = null) : Exception(message, cause)
    {
        public class DuplicatePersonException(string message, Exception? cause = null)
            : PersonRepositoryException(message, cause);
    }
}