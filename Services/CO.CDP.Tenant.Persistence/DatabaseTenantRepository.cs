using Microsoft.EntityFrameworkCore;
using static CO.CDP.Tenant.Persistence.ITenantRepository.TenantRepositoryException;

namespace CO.CDP.Tenant.Persistence;

public class DatabaseTenantRepository(TenantContext context) : ITenantRepository
{
    public void Save(Tenant tenant)
    {
        try
        {
            context.Add(tenant);
            context.SaveChanges();
        }
        catch (DbUpdateException cause)
        {
            HandleDbUpdateException(tenant, cause);
        }
    }

    public async Task<Tenant?> Find(Guid tenantId)
    {
        return await context.Tenants.FirstOrDefaultAsync(t => t.Guid == tenantId);
    }

    public void Dispose()
    {
        context.Dispose();
    }

    private static void HandleDbUpdateException(Tenant tenant, DbUpdateException cause)
    {
        switch (cause.InnerException)
        {
            case { } e when e.ContainsDuplicateKey("_Tenants_Name"):
                throw new DuplicateTenantException($"Tenant with name `{tenant.Name}` already exists.", cause);
            case { } e when e.ContainsDuplicateKey("_Tenants_Guid"):
                throw new DuplicateTenantException($"Tenant with guid `{tenant.Guid}` already exists.", cause);
            default: throw cause;
        }
    }
}

internal static class StringExtensions
{
    internal static bool ContainsDuplicateKey(this Exception cause, string name) =>
        cause.Message.ContainsDuplicateKey(name);

    private static bool ContainsDuplicateKey(this string message, string name) =>
        message.Contains("duplicate key value violates unique constraint") &&
        message.Contains($"{name}\"");
}
