using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.TestKit.Mvc;
using Xunit.Abstractions;

namespace CO.CDP.Tenant.WebApiClient.Tests;

public class TenantClientIntegrationTest(ITestOutputHelper testOutputHelper)
{
    private readonly TestWebApplicationFactory<Program> _factory = new(builder =>
    {
        builder.ConfigureInMemoryDbContext<OrganisationInformationContext>();
        builder.ConfigureLogging(testOutputHelper);
    });

    [Fact]
    public async Task ItTalksToTheTenantApi()
    {
        ITenantClient client = new TenantClient("https://localhost", _factory.CreateClient());

        var tenant = await client.CreateTenantAsync(new NewTenant(
            name: $"Bob {Guid.NewGuid()}"
        ));

        var foundTenant = await client.GetTenantAsync(tenant.Id);

        Assert.Equal(
            new Tenant
            (
                id: tenant.Id,
                name: tenant.Name
            ),
            foundTenant
        );
    }
}