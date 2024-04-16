namespace CO.CDP.Person.Persistence;

public interface IPersonRepository : IDisposable
{
    public void Save(Person person);

    public Task<Person?> Find(Guid personId);

    public Task<Person?> FindByName(string name);
    public Task<Person?> FindByEmail(string emailAddress);

    public class PersonRepositoryException(string message, Exception? cause = null) : Exception(message, cause)
    {
        public class DuplicatePersonException(string message, Exception? cause = null)
            : PersonRepositoryException(message, cause);
    }
}