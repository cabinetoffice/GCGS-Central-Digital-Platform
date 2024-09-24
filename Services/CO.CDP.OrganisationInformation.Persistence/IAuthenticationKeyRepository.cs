namespace CO.CDP.OrganisationInformation.Persistence;

public interface IAuthenticationKeyRepository : IDisposable
{
    public Task Save(AuthenticationKey authenticationKey);
    public Task<AuthenticationKey?> Find(string key);
    public Task<IEnumerable<AuthenticationKey?>> GetAuthenticationKeys(Guid organisationId);

    public Task<AuthenticationKey?> FindByKeyNameAndOrganisationId(string name, Guid organisationId);
    public class AuthenticationKeyRepositoryException(string message, Exception? cause = null) : Exception(message, cause)
    {
        public class DuplicateAuthenticationKeyNameException(string message, Exception? cause = null)
            : AuthenticationKeyRepositoryException(message, cause);        
    }
}