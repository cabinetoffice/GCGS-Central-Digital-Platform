namespace CO.CDP.Tenant.Persistence;

public interface ITenantRepository : IDisposable
{
    public void Save(Tenant tenant);

    public Task<Tenant?> Find(Guid tenantId);
}