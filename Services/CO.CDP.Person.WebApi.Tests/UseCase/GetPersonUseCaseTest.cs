using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Person.WebApi.Tests.AutoMapper;
using CO.CDP.Person.WebApi.UseCase;
using FluentAssertions;
using Moq;

namespace CO.CDP.Person.WebApi.Tests.UseCase;

public class GetPersonUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IPersonRepository> _repository = new();
    private GetPersonUseCase UseCase => new(_repository.Object, mapperFixture.Mapper);

    [Fact]
    public async Task ItReturnsNullIfNoPersonIsFound()
    {
        var personId = Guid.NewGuid();

        var found = await UseCase.Execute(personId);

        found.Should().BeNull();
    }

    [Fact]
    public async Task ItReturnsTheFoundPerson()
    {
        var persontId = Guid.NewGuid();
        var scopes = new List<string>();
        var tenant = new OrganisationInformation.Persistence.Person
        {
            Id = 42,
            Guid = persontId,
            Email = "person@example.com",
            FirstName = "fn",
            LastName = "ln",
            Scopes = scopes
        };

        _repository.Setup(r => r.Find(persontId)).ReturnsAsync(tenant);

        var found = await UseCase.Execute(persontId);

        found.Should().BeEquivalentTo(new Model.Person
        {
            Id = persontId,
            FirstName = "fn",
            LastName = "ln",
            Email = "person@example.com",
            Scopes = scopes
        });
    }
}