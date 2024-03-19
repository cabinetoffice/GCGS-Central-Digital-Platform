namespace CO.CDP.Tenant.Persistence;

public class InMemoryTenantRepository : ITenantRepository
{
    private readonly Dictionary<int, Tenant> _tenants = new();

    public void Dispose()
    {
    }

    public void Save(Tenant tenant)
    {
        GuardTenantIsUnique(tenant);

        if (tenant.Id == 0)
        {
            tenant.Id = _tenants.Count + 1;
        }

        _tenants[tenant.Id] = tenant;
    }

    public Task<Tenant?> Find(Guid tenantId)
    {
        return Task.FromResult(
            _tenants.FirstOrDefault(t => t.Value.Guid == tenantId).Value
        )!;
    }

    private void GuardTenantIsUnique(Tenant tenant)
    {
        var tenantByGuid = _tenants.FirstOrDefault(t => t.Value.Guid == tenant.Guid).Value;
        if (tenantByGuid != null && tenantByGuid.Id != tenant.Id)
        {
            throw new ITenantRepository.TenantRepositoryException.DuplicateTenantException(
                $"Tenant with guid `{tenant.Guid}` already exists.");
        }

        var tenantByName = _tenants.FirstOrDefault(t => t.Value.Name == tenant.Name).Value;
        if (tenantByName != null && tenantByName.Id != tenant.Id)
        {
            throw new ITenantRepository.TenantRepositoryException.DuplicateTenantException(
                $"Tenant with name `{tenant.Name}` already exists.");
        }
    }
}