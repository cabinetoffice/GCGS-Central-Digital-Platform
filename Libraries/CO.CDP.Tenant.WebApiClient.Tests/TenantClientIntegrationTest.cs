namespace CO.CDP.Tenant.WebApiClient.Tests;

public class TenantClientIntegrationTest
{
    [Fact(Skip = "The test requires the tenant service to run.")]
    public async Task ItTalksToTheTenantApi()
    {
        ITenantClient client = new TenantClient("http://localhost:5182", new HttpClient());

        var tenant = await client.CreateTenantAsync(new NewTenant(
            name: $"Bob {Guid.NewGuid()}",
            contactInfo: new TenantContactInfo(
                email: "bob@example.com",
                phone: "07923234234"
            )
        ));

        var foundTenant = await client.GetTenantAsync(tenant.Id);

        Assert.Equal(
            new Tenant
            (
                id: tenant.Id,
                name: tenant.Name,
                contactInfo: new TenantContactInfo(
                    email: "bob@example.com",
                    phone: "07923234234"
                )
            ),
            foundTenant
        );
    }
}