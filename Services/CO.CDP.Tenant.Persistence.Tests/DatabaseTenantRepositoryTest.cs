using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.Tenant.Persistence.Tests;

public class DatabaseTenantRepositoryTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task ItFindsSavedTenant()
    {
        using var repository = TenantRepository();

        var tenant = GivenTenant() with { Guid = Guid.NewGuid() };

        repository.Save(tenant);

        var found = await repository.Find(tenant.Guid);

        found.Should().Be(tenant);
        found.As<Tenant>().Id.Should().BePositive();
    }

    [Fact]
    public async Task ItReturnsNullIfTenantIsNotFound()
    {
        using var repository = TenantRepository();

        var found = await repository.Find(Guid.NewGuid());

        found.Should().BeNull();
    }

    [Fact]
    public void ItRejectsTwoTenantsWithTheSameName()
    {
        using var repository = TenantRepository();

        var tenant1 = GivenTenant() with { Guid = Guid.NewGuid(), Name = "Bob" };
        var tenant2 = GivenTenant() with { Guid = Guid.NewGuid(), Name = "Bob" };

        repository.Save(tenant1);

        repository.Invoking(r => r.Save(tenant2))
            .Should().Throw<ITenantRepository.TenantRepositoryException.DuplicateTenantException>()
            .WithMessage($"Tenant with name `Bob` already exists.");
    }

    [Fact]
    public void ItRejectsTwoTenantsWithTheSameGuid()
    {
        using var repository = TenantRepository();

        var guid = Guid.NewGuid();
        var tenant1 = GivenTenant() with { Guid = guid, Name = "Alice" };
        var tenant2 = GivenTenant() with { Guid = guid, Name = "Sussan" };

        repository.Save(tenant1);

        repository.Invoking((r) => r.Save(tenant2))
            .Should().Throw<ITenantRepository.TenantRepositoryException.DuplicateTenantException>()
            .WithMessage($"Tenant with guid `{guid}` already exists.");
    }

    [Fact]
    public async Task ItUpdatesAnExistingTenant()
    {
        using var repository = TenantRepository();

        var tenant = GivenTenant() with { Guid = Guid.NewGuid(), Name = "Olivia" };

        repository.Save(tenant);
        tenant.Name = "Hannah";
        repository.Save(tenant);

        var found = await repository.Find(tenant.Guid);

        found.Should().Be(tenant);
    }

    private ITenantRepository TenantRepository()
    {
        return new DatabaseTenantRepository(TenantContext());
    }

    private TenantContext TenantContext()
    {
        var context = new TenantContext(postgreSql.ConnectionString);
        context.Database.Migrate();
        context.SaveChanges();
        return context;
    }

    private static Tenant GivenTenant()
    {
        var guid = Guid.NewGuid();
        return new Tenant
        {
            Guid = guid,
            Name = $"Stefan {guid}",
            ContactInfo = new Tenant.TenantContactInfo
            {
                Email = "stefan@example.com",
                Phone = "07925123123"
            }
        };
    }
}