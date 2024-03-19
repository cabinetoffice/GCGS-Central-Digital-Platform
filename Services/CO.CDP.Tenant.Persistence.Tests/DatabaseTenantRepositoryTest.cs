using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using static CO.CDP.Tenant.Persistence.ITenantRepository.TenantRepositoryException;

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

        found.Should().Be(tenant);
    }

    [Fact]
    public void ItRejectsTwoTenantsWithTheSameName()
    {
        using var repository = new DatabaseTenantRepository(TenantContext());

        var tenant1 = GivenTenant(guid: Guid.NewGuid(), name: "Bob");
        var tenant2 = GivenTenant(guid: Guid.NewGuid(), name: "Bob");

        repository.Save(tenant1);

        repository.Invoking(r => r.Save(tenant2))
            .Should().Throw<DuplicateTenantException>()
            .WithMessage($"Tenant with name `Bob` already exists.");
    }

    [Fact]
    public void ItRejectsTwoTenantsWithTheSameGuid()
    {
        using var repository = new DatabaseTenantRepository(TenantContext());

        var guid = Guid.NewGuid();
        var tenant1 = GivenTenant(guid: guid, name: "Alice");
        var tenant2 = GivenTenant(guid: guid, name: "Sussan");

        repository.Save(tenant1);

        repository.Invoking((r) => r.Save(tenant2))
            .Should().Throw<DuplicateTenantException>()
            .WithMessage($"Tenant with guid `{guid}` already exists.");
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
}