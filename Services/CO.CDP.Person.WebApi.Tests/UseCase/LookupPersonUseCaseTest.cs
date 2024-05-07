using CO.CDP.Person.WebApi.Tests.AutoMapper;
using CO.CDP.Person.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;

namespace CO.CDP.Person.WebApi.Tests.UseCase;
public class LookupPersonUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IPersonRepository> _repository = new();
    private LookupPersonUseCase UseCase => new(_repository.Object, mapperFixture.Mapper);

    [Fact]
    public async Task Execute_IfNoPersonIsFound_ReturnsNull()
    {
        var name = "Test Person";

        var found = await UseCase.Execute(name);

        found.Should().BeNull();
    }

    [Fact]
    public async Task Execute_IfPersonIsFound_ReturnsPerson()
    {
        var personId = Guid.NewGuid();
        var persistencePerson = new OrganisationInformation.Persistence.Person
        {
            Id = 1,
            Guid = personId,
            FirstName = "fn",
            LastName = "ln",
            Age = 40,
            Email = "email@email.com"
        };

        _repository.Setup(r => r.FindByEmail(persistencePerson.Email)).ReturnsAsync(persistencePerson);

        var found = await UseCase.Execute("email@email.com");

        found.Should().BeEquivalentTo(new Model.Person
        {
            Id = personId,
            Email = "email@email.com",
            FirstName = "fn",
            LastName = "ln",
            Age = 40,
        }, options => options.ComparingByMembers<Model.Person>());
    }
}
