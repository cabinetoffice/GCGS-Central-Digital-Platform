namespace CO.CDP.OrganisationInformation.Persistence;

public interface ITenantRepository : IDisposable
{
    public void Save(Tenant tenant);

    public Task<Tenant?> Find(Guid tenantId);

    public Task<Tenant?> FindByName(string name);
    public Task<TenantLookup?> LookupTenant(string userUrn);

    public class TenantRepositoryException(string message, Exception? cause = null) : Exception(message, cause)
    {
        public class DuplicateTenantException(string message, Exception? cause = null)
            : TenantRepositoryException(message, cause);
        public class TenantNotFoundException(string message, Exception? cause = null)
            : TenantRepositoryException(message, cause);
    }
}