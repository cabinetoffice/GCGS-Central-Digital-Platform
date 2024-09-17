namespace CO.CDP.OrganisationInformation.Persistence;

public interface IAuthenticationKeyRepository : IDisposable
{
    public Task Save(AuthenticationKey authenticationKey);
    public Task<AuthenticationKey?> Find(string key);
    public Task<IEnumerable<AuthenticationKey?>> GetAuthenticationKeys(Guid organisationId);
}