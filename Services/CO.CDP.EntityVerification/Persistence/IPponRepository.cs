namespace CO.CDP.EntityVerification.Persistence;

public interface IPponRepository 
{
    public void Save(EntityVerificationContext context, Ppon identifier);

    public class PponRepositoryException(string message, Exception? cause = null) : Exception(message, cause)
    {
        public class DuplicatePponException(string message, Exception? cause = null)
            : PponRepositoryException(message, cause);
    }
}