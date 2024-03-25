using CO.CDP.Tenant.WebApiClient;

namespace CO.CDP.OrganisationApp.ServiceClient;

public class FakeTenantClient : ITenantClient
{
    public Task<Tenant.WebApiClient.Tenant?> GetTenant(Guid tenantId)
    {
        return Task.FromResult((Tenant.WebApiClient.Tenant?)tenant);
    }

    public Task<Tenant.WebApiClient.Tenant> RegisterTenant(NewTenant newTenant)
    {
        return Task.FromResult(tenant);
    }

    static readonly Tenant.WebApiClient.Tenant tenant = new()
    {
        Id = Guid.NewGuid(),
        Name = "Dummy",
        ContactInfo = new TenantContactInfo
        {
            Email = "dummy@test.com",
            Phone = "01406946277"
        }
    };
}