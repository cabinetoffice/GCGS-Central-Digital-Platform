using CO.CDP.Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.Tenant.Persistence.Tests;

public class DatabaseTenantRepositoryTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task ItFindsSavedTenant()
    {
        using var repository = new DatabaseTenantRepository(TenantContext());

        var tenant = GivenTenant(guid: Guid.NewGuid());

        repository.Save(tenant);

        var found = await repository.Find(tenant.Guid);

        Assert.Equal(tenant, found);
    }

    [Fact]
    public void ItRejectsTwoTenantsWithTheSameName()
    {
        using var repository = new DatabaseTenantRepository(TenantContext());

        var tenant1 = GivenTenant(guid: Guid.NewGuid(), name: "Bob");
        var tenant2 = GivenTenant(guid: Guid.NewGuid(), name: "Bob");

        repository.Save(tenant1);

        AssertDuplicateKeyViolation("Tenants_Name", () => repository.Save(tenant2));
    }

    private static Tenant GivenTenant(
        Guid? guid,
        string name = "Stefan",
        string email = "stefan@example.com",
        string phone = "07925123123")
    {
        var theGuid = guid ?? Guid.NewGuid();
        return new Tenant
        {
            Guid = theGuid,
            Name = name,
            ContactInfo = new Tenant.TenantContactInfo
            {
                Email = email,
                Phone = phone
            }
        };
    }

    private TenantContext TenantContext()
    {
        var context = new TenantContext(postgreSql.ConnectionString);
        context.Database.Migrate();
        context.SaveChanges();
        return context;
    }

    private void AssertDuplicateKeyViolation(string indexName, Action action)
    {
        var exception = Assert.Throws<DbUpdateException>(action);
        Assert.IsType<Npgsql.PostgresException>(exception.InnerException);
        Assert.Contains("duplicate key value violates unique constraint", exception.InnerException.Message);
        Assert.Contains(indexName, exception.InnerException.Message);
    }
}