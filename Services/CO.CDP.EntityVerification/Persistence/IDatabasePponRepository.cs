namespace CO.CDP.EntityVerification.Persistence;

public interface IDatabasePponRepository : IDisposable
{
    public void Save(Ppon identifier);

    public class PponRepositoryException(string message, Exception? cause = null) : Exception(message, cause)
    {
        public class DuplicatePponException(string message, Exception? cause = null)
            : PponRepositoryException(message, cause);
    }
}