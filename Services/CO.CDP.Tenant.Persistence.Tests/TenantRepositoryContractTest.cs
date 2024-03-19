using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using static CO.CDP.Tenant.Persistence.ITenantRepository.TenantRepositoryException;

namespace CO.CDP.Tenant.Persistence.Tests;

public abstract class TenantRepositoryContractTest
{
    [Fact]
    public async Task ItFindsSavedTenant()
    {
        using var repository = TenantRepository();

        var tenant = GivenTenant(guid: Guid.NewGuid());

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
        using var repository = TenantRepository();

        var guid = Guid.NewGuid();
        var tenant1 = GivenTenant(guid: guid, name: "Alice");
        var tenant2 = GivenTenant(guid: guid, name: "Sussan");

        repository.Save(tenant1);

        repository.Invoking((r) => r.Save(tenant2))
            .Should().Throw<DuplicateTenantException>()
            .WithMessage($"Tenant with guid `{guid}` already exists.");
    }

    [Fact]
    public async Task ItUpdatesAnExistingTenant()
    {
        using var repository = TenantRepository();

        var tenant = GivenTenant(guid: Guid.NewGuid(), name: "Olivia");

        repository.Save(tenant);
        tenant.Name = "Hannah";
        repository.Save(tenant);

        var found = await repository.Find(tenant.Guid);

        found.Should().Be(tenant);
    }

    protected abstract ITenantRepository TenantRepository();

    private static Tenant GivenTenant(
        Guid? guid = null,
        string? name = null,
        string email = "stefan@example.com",
        string phone = "07925123123")
    {
        var theGuid = guid ?? Guid.NewGuid();
        var theName = name ?? $"Stefan {theGuid}";
        return new Tenant
        {
            Guid = theGuid,
            Name = theName,
            ContactInfo = new Tenant.TenantContactInfo
            {
                Email = email,
                Phone = phone
            }
        };
    }
}