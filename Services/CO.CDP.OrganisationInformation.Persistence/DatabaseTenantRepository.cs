using Microsoft.EntityFrameworkCore;
using static CO.CDP.OrganisationInformation.Persistence.ITenantRepository;

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

    public async Task<TenantLookup?> LookupTenant(string userUrn)
    {
        try
        {
            return await context.Persons
                 .Where(p => p.UserUrn == userUrn)
                 .Select(p => new TenantLookup
                 {
                     User = new TenantLookup.PersonUser
                     {
                         Email = p.Email,
                         Urn = p.UserUrn ?? "",
                         Name = $"{p.FirstName} {p.LastName}"
                     },
                     Tenants = p.Tenants.Select(t => new TenantLookup.Tenant
                     {
                         Id = t.Guid,
                         Name = t.Name,
                         Organisations = t.Organisations.Select(o => new TenantLookup.Organisation
                         {
                             Id = o.Guid,
                             Name = o.Name,
                             Roles = o.Roles,
                             PendingRoles = o.PendingRoles,
                             // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
                             Scopes = o.OrganisationPersons.Single(op => op.PersonId == p.Id).Scopes ?? new List<string>(),
                             ApprovedOn = o.ApprovedOn

                         }).ToList()
                     }).ToList()
                 })
                 .SingleAsync();

        }
        catch (InvalidOperationException ex)
        {
            throw new TenantRepositoryException.TenantNotFoundException($"Tenant not found for the given URN: {userUrn}", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new TenantRepositoryException("An error occurred while looking up the tenant.", ex);
        }
        catch (Exception ex)
        {
            throw new TenantRepositoryException("An unexpected error occurred while looking up the tenant.", ex);
        }
    }

    public void Dispose()
    {
        context.Dispose();
    }

    private static void HandleDbUpdateException(Tenant tenant, DbUpdateException cause)
    {
        switch (cause.InnerException)
        {
            case { } e when e.ContainsDuplicateKey("_tenants_name"):
                throw new ITenantRepository.TenantRepositoryException.DuplicateTenantException(
                    $"Tenant with name `{tenant.Name}` already exists.", cause);
            case { } e when e.ContainsDuplicateKey("_tenants_guid"):
                throw new ITenantRepository.TenantRepositoryException.DuplicateTenantException(
                    $"Tenant with guid `{tenant.Guid}` already exists.", cause);
            default: throw cause;
        }
    }
}