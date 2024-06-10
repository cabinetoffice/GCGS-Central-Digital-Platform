using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

public class DatabaseTenantRepository(OrganisationInformationContext context) : ITenantRepository
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

    public async Task<Person?> FindByUserUrn(string userUrn)
    {
        return await context.Persons
                    .Include(p => p.Tenants)
                    .Include(p => p.Organisations)
                    .FirstOrDefaultAsync(p => p.UserUrn == userUrn);
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
                throw new ITenantRepository.TenantRepositoryException.DuplicateTenantException(
                    $"Tenant with name `{tenant.Name}` already exists.", cause);
            case { } e when e.ContainsDuplicateKey("_Tenants_Guid"):
                throw new ITenantRepository.TenantRepositoryException.DuplicateTenantException(
                    $"Tenant with guid `{tenant.Guid}` already exists.", cause);
            default: throw cause;
        }
    }
}