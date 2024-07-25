namespace CO.CDP.EntityVerification.Persistence;

public interface IPponRepository : IDisposable
{
    public void Save(Ppon identifier);
    public Task<Ppon?> FindPponByPponIdAsync(string pponId);
    public Task<Ppon?> FindPponByIdentifierAsync(IEnumerable<Events.Identifier> pponIdentifiers);
    void UpdatePponIdentifiersAsync(Ppon pponToUpdate, IEnumerable<Events.Identifier> identifiers);

    public class PponRepositoryException(string message, Exception? cause = null) : Exception(message, cause)
    {
        public class DuplicatePponException(string message, Exception? cause = null)
            : PponRepositoryException(message, cause);
    }
}