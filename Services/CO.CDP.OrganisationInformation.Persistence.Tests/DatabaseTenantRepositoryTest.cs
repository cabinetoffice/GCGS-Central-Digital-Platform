using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using static CO.CDP.OrganisationInformation.Persistence.Tests.EntityFactory;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class DatabaseTenantRepositoryTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task ItFindsSavedTenant()
    {
        using var repository = TenantRepository();

        var person = GivenPerson();
        var organisation = GivenOrganisation();
        var tenant = GivenTenant(guid: Guid.NewGuid(), person: person, organisation: organisation);

        repository.Save(tenant);

        var found = await repository.Find(tenant.Guid);

        found.Should().Be(tenant);
        found.As<Tenant>().Id.Should().BePositive();
        found.As<Tenant>().Organisations.Should().Contain(o => o.Guid == organisation.Guid);
        found.As<Tenant>().Persons.Should().Contain(p => p.Guid == person.Guid);
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
            .Should().Throw<ITenantRepository.TenantRepositoryException.DuplicateTenantException>()
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
            .Should().Throw<ITenantRepository.TenantRepositoryException.DuplicateTenantException>()
            .WithMessage($"Tenant with guid `{guid}` already exists.");
    }

    [Fact]
    public async Task ItUpdatesAnExistingTenant()
    {
        using var repository = TenantRepository();

        var tenant = GivenTenant(guid: Guid.NewGuid(), name: "Olivia");
        var initialDate = DateTime.UtcNow.AddDays(-1);

        repository.Save(tenant);
        tenant.Name = "Hannah";
        repository.Save(tenant);

        var found = await repository.Find(tenant.Guid);

        found.Should().Be(tenant);
        found.As<Tenant>().UpdatedOn.Should().BeAfter(initialDate);
    }

    [Fact]
    public async Task FindByName_WhenFound_ReturnsTenant()
    {
        using var repository = TenantRepository();

        var tenant = GivenTenant(name: "urn:fdc:gov.uk:2022:43af5a8b-f4c0-414b-b341-d4f1fa894302");

        repository.Save(tenant);

        var found = await repository.FindByName(tenant.Name);

        found.Should().Be(tenant);
        found.As<Tenant>().Id.Should().BePositive();
    }

    [Fact]
    public async Task FindByName_WhenNotFound_ReturnsNull()
    {
        using var repository = TenantRepository();

        var found = await repository.FindByName("urn:fdc:gov.uk:2022:43af5a8b-f4c0-414b-b341-d4f1fa894301");

        found.Should().BeNull();
    }

    [Fact]
    public async Task ItCorrectlyAssociatesTenantWithMultiplePersons()
    {
        using var repository = TenantRepository();

        var tenant = GivenTenant(guid: Guid.NewGuid(), name: "Tenant with Persons");
        var person1 = GivenPerson(guid: Guid.NewGuid(), email: "person1@example.com");
        var person2 = GivenPerson(guid: Guid.NewGuid(), email: "person2@example.com");
        tenant.Persons.Add(person1);
        tenant.Persons.Add(person2);

        repository.Save(tenant);
        var found = await repository.Find(tenant.Guid);

        found.Should().NotBeNull();
        found.As<Tenant>().Persons.Should().HaveCount(2);
        found.As<Tenant>().Persons.Should().Contain(p => p.Guid == person1.Guid);
        found.As<Tenant>().Persons.Should().Contain(p => p.Guid == person2.Guid);
    }

    [Fact]
    public async Task FindByUserUrn_WhenPersonExistsWithTenantsAndOrganisations_ReturnsPersonWithAssociations()
    {
        using var repository = TenantRepository();

        var userUrn = "urn:fdc:gov.uk:2022:43af5a8b-f4c0-414b-b341-d4f1fa894302";
        var person = GivenPerson(guid: Guid.NewGuid(), email: "Person1@example.com", userUrn: userUrn);
        var organisation1 = GivenOrganisation(guid: Guid.NewGuid(), name: "Acme Corporation");
        var organisation2 = GivenOrganisation(guid: Guid.NewGuid(), name: "Widget Co");
        var tenant1 = GivenTenant(guid: Guid.NewGuid(), name: "Acme Corporation / John Doe", person: person, organisation: organisation1);
        var tenant2 = GivenTenant(guid: Guid.NewGuid(), name: "Widget Co / John Doe", person: person, organisation: organisation2);

        repository.Save(tenant1);
        repository.Save(tenant2);

        var foundPerson = await repository.FindByUserUrn(userUrn);

        foundPerson.Should().NotBeNull();
        foundPerson.As<Person>().UserUrn.Should().Be(userUrn);
        foundPerson.As<Person>().Tenants.Should().HaveCount(2);
        foundPerson.As<Person>().Tenants?.FirstOrDefault()?.Organisations.Should().HaveCount(1);
    }

    private ITenantRepository TenantRepository()
    {
        return new DatabaseTenantRepository(postgreSql.OrganisationInformationContext());
    }
}