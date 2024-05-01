using Microsoft.EntityFrameworkCore;
using static CO.CDP.Persistence.OrganisationInformation.ITenantRepository.TenantRepositoryException;

namespace CO.CDP.Persistence.OrganisationInformation;

public class DatabaseTenantRepository(TenantContext context) : ITenantRepository
{
    public void Save(Tenant tenant)
    {
        try
        {
            context.Update(tenant);
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

    public async Task<Tenant?> FindByName(string name)
    {
        return await context.Tenants.FirstOrDefaultAsync(t => t.Name == name);
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
                throw new ITenantRepository.TenantRepositoryException.DuplicateTenantException($"Tenant with name `{tenant.Name}` already exists.", cause);
            case { } e when e.ContainsDuplicateKey("_Tenants_Guid"):
                throw new ITenantRepository.TenantRepositoryException.DuplicateTenantException($"Tenant with guid `{tenant.Guid}` already exists.", cause);
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
