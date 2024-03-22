namespace CO.CDP.Tenant.Persistence;

public interface ITenantRepository : IDisposable
{
    public void Save(Tenant tenant);

    public Task<Tenant?> Find(Guid tenantId);

    public class TenantRepositoryException(string message, Exception? cause = null) : Exception(message, cause)
    {
        public class DuplicateTenantException(string message, Exception? cause = null)
            : TenantRepositoryException(message, cause);
    }
}