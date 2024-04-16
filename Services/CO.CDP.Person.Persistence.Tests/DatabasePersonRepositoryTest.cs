using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;

namespace CO.CDP.Person.Persistence.Tests;

public class DatabasePersonRepositoryTest : IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture postgreSql;

    public DatabasePersonRepositoryTest(PostgreSqlFixture postgreSql)
    {
        this.postgreSql = postgreSql;
    }

    [Fact]
    public async Task ItFindsSavedPerson()
    {
        using var repository = PersonRepository();
        var person = GivenPerson();

        repository.Save(person);

        var found = await repository.Find(person.Guid);

        found.Should().BeEquivalentTo(person);
    }

    [Fact]
    public async Task ItReturnsNullIfPersonIsNotFound()
    {
        using var repository = PersonRepository();

        var found = await repository.Find(Guid.NewGuid());

        found.Should().BeNull();
    }

    [Fact]
    public void ItRejectsTwoPersonsWithTheSameName()
    {
        using var repository = PersonRepository();

        var name = "DuplicateName";
        var person1 = GivenPerson(name: name);
        var person2 = GivenPerson(name: name);

        repository.Save(person1);

        Action act = () => repository.Save(person2);

        act.Should().Throw<IPersonRepository.PersonRepositoryException.DuplicatePersonException>();
    }

    [Fact]
    public async Task ItUpdatesAnExistingPerson()
    {
        using var repository = PersonRepository();

        var person = GivenPerson();
        repository.Save(person);
        person.Name = "Updated Name";
        repository.Save(person);

        var found = await repository.Find(person.Guid);

        found.Should().NotBeNull();
        found!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task FindByName_WhenFound_ReturnsPerson()
    {
        using var repository = PersonRepository();

        var person = GivenPerson(name: "UniqueName");
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
        return new DatabasePersonRepository(PersonContext());
    }

    private PersonContext PersonContext()
    {
        return new PersonContext(postgreSql.ConnectionString);
    }

    private static Person GivenPerson(Guid? guid = null, string name = "Default Name", int age = 30, string email = "default@example.com")
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
