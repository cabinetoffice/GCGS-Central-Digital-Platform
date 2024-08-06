namespace CO.CDP.EntityVerification.Persistence;

public interface IPponRepository : IDisposable
{
    public void Save(Ppon identifier);
    public Task<Ppon?> FindPponByPponIdAsync(string pponId);
    public Task<Ppon?> FindPponByIdentifierAsync(string scheme, string id);
    public class PponRepositoryException(string message, Exception? cause = null) : Exception(message, cause)
    {
        public class DuplicatePponException(string message, Exception? cause = null)
            : PponRepositoryException(message, cause);
    }
}