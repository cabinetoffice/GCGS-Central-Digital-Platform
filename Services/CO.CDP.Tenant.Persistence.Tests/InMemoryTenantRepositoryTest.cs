namespace CO.CDP.Tenant.Persistence.Tests;

public class InMemoryTenantRepositoryTest : TenantRepositoryContractTest
{
    protected override ITenantRepository TenantRepository()
    {
        return new InMemoryTenantRepository();
    }
}