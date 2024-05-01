namespace CO.CDP.Persistence.OrganisationInformation;

public interface ITenantRepository : IDisposable
{
    public void Save(Tenant tenant);

    public Task<Tenant?> Find(Guid tenantId);

    public Task<Tenant?> FindByName(string name);

    public class TenantRepositoryException(string message, Exception? cause = null) : Exception(message, cause)
    {
        public class DuplicateTenantException(string message, Exception? cause = null)
            : TenantRepositoryException(message, cause);
    }
}