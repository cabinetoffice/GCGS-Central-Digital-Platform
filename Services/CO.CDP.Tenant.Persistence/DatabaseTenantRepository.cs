using Microsoft.EntityFrameworkCore;

namespace CO.CDP.Tenant.Persistence;

public class DatabaseTenantRepository(TenantContext context) : ITenantRepository
{
    public void Save(Tenant tenant)
    {
        context.Add(tenant);
        context.SaveChanges();
    }

    public async Task<Tenant?> Find(Guid tenantId)
    {
        return await context.Tenants.FirstOrDefaultAsync(t => t.Guid == tenantId);
    }

    public void Dispose()
    {
        context.Dispose();
    }
}