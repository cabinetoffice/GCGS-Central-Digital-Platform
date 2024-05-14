using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using static CO.CDP.OrganisationInformation.Persistence.Tests.EntityFactory;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class DatabasePersonRepositoryTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{

    [Fact]
    public async Task ItFindsSavedPerson()
    {
        using var repository = PersonRepository();

        var tenant = GivenTenant();
        var person = GivenPerson(guid: Guid.NewGuid(), tenant: tenant);

        repository.Save(person);

        var found = await repository.Find(person.Guid);

        found.Should().Be(person);
        found.As<Person>().Id.Should().BePositive();
        found.As<Person>().Tenants.Should().Contain(t => t.Guid == tenant.Guid);
    }

    [Fact]
    public async Task ItReturnsNullIfPersonIsNotFound()
    {
        using var repository = PersonRepository();

        var found = await repository.Find(Guid.NewGuid());

        found.Should().BeNull();
    }

    [Fact]
    public void ItRejectsTwoPersonsWithTheSameEmail()
    {
        using var repository = PersonRepository();

        var email = "duplicate@email.com";
        var Person1 = GivenPerson(guid: Guid.NewGuid(), email: email);
        var Person2 = GivenPerson(guid: Guid.NewGuid(), email: email);

        repository.Save(Person1);

        repository.Invoking(r => r.Save(Person2))
            .Should().Throw<IPersonRepository.PersonRepositoryException.DuplicatePersonException>()
            .WithMessage($"Person with email `{email}` already exists.");
    }

    [Fact]
    public void ItRejectsTwoPersonsWithTheSameGuid()
    {
        using var repository = PersonRepository();

        var guid = Guid.NewGuid();
        var Person1 = GivenPerson(guid: guid, email: "Person1@example.com");
        var Person2 = GivenPerson(guid: guid, email: "Person2@example.com");

        repository.Save(Person1);

        repository.Invoking((r) => r.Save(Person2))
            .Should().Throw<IPersonRepository.PersonRepositoryException.DuplicatePersonException>()
            .WithMessage($"Person with guid `{guid}` already exists.");
    }

    [Fact]
    public async Task ItUpdatesAnExistingPerson()
    {
        using var repository = PersonRepository();

        var person = GivenPerson();
        person.Email = "email@email.com";
        repository.Save(person);

        var found = await repository.Find(person.Guid);

        found.Should().NotBeNull();
        found!.Email.Should().Be("email@email.com");
    }

    [Fact]
    public async Task ItCorrectlyAssociatesPersonWithOrganisation()
    {
        using var repository = PersonRepository();

        var person = GivenPerson(guid: Guid.NewGuid(), email: "Person3@example.com");
        var tenant = GivenTenant();
        var organisation = GivenOrganisation(guid: Guid.NewGuid(), name: "TheOrganisation");
        person.Organisations.Add(organisation);

        repository.Save(person);
        var found = await repository.Find(person.Guid);

        found.Should().NotBeNull();
        found?.Organisations.Should().Contain(org => org.Guid == organisation.Guid);
    }


    private IPersonRepository PersonRepository()
    {
        return new DatabasePersonRepository(OrganisationInformationContext());
    }

    private OrganisationInformationContext OrganisationInformationContext()
    {
        var options = new DbContextOptionsBuilder<OrganisationInformationContext>()
            .UseNpgsql(postgreSql.ConnectionString)
            .Options;
        var context = new OrganisationInformationContext(options);
        context.Database.Migrate();
        context.SaveChanges();
        return context;
    }
}
