using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.Persistence.OrganisationInformation.Tests;

public class DatabasePersonRepositoryTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{

    [Fact]
    public async Task ItFindsSavedPerson()
    {
        using var repository = PersonRepository();

        var Person = GivenPerson(guid: Guid.NewGuid());

        repository.Save(Person);

        var found = await repository.Find(Person.Guid);

        found.Should().Be(Person);
        found.As<Person>().Id.Should().BePositive();

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
    public async Task FindByName_WhenFound_ReturnsPerson()
    {
        using var repository = PersonRepository();

        var person = GivenPerson(name: "UniqueName");
        person.Email = "email1@email.com";
        repository.Save(person);

        var found = await repository.FindByName(person.Name);

        found.Should().BeEquivalentTo(person);
    }

    [Fact]
    public async Task FindByName_WhenNotFound_ReturnsNull()
    {
        using var repository = PersonRepository();

        var found = await repository.FindByName("NonExistentName");

        found.Should().BeNull();
    }

    private IPersonRepository PersonRepository()
    {
        return new DatabasePersonRepository(OrganisationInformationContext());
    }

    private OrganisationInformationContext OrganisationInformationContext()
    {
        var context = new OrganisationInformationContext(postgreSql.ConnectionString);
        context.Database.Migrate();
        context.SaveChanges();
        return context;
    }

    private static Person GivenPerson(Guid? guid = null, string name = "Jon Doe", int age = 40, string email = "jon@example.com")
    {
        return new Person
        {
            Guid = guid ?? Guid.NewGuid(),
            Name = name,
            Age = age,
            Email = email
        };
    }

}
