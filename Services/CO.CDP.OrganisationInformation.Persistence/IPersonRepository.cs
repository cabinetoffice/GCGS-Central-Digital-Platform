namespace CO.CDP.OrganisationInformation.Persistence;

public interface IPersonRepository : IDisposable
{
    public void Save(Person person);

    public Task<Person?> Find(Guid personId);

    public Task<Person?> FindByUrn(string urn);
    public Task<Person?> FindByEmail(string email);

    public Task<IEnumerable<Person>> FindByOrganisation(Guid organisationId);

    public class PersonRepositoryException(string message, Exception? cause = null) : Exception(message, cause)
    {
        public class DuplicateGuidException(string message, Exception? cause = null)
            : PersonRepositoryException(message, cause);

        public class DuplicateEmailException(string message, Exception? cause = null)
            : PersonRepositoryException(message, cause);
    }
}