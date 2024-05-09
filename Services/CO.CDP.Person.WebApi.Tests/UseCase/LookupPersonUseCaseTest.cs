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
        var urn = "urn:fdc:gov.uk:2022:7wTqYGMFQxgukTSpSI2GodMwe9";

        var found = await UseCase.Execute(urn);

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
            Email = "email@email.com",
            UserUrn = "urn:fdc:gov.uk:2022:7wTqYGMFQxgukTSpSI2GodMwe9"
        };

        _repository.Setup(r => r.FindByUrn(persistencePerson.UserUrn)).ReturnsAsync(persistencePerson);

        var found = await UseCase.Execute("urn:fdc:gov.uk:2022:7wTqYGMFQxgukTSpSI2GodMwe9");

        found.Should().BeEquivalentTo(new Model.Person
        {
            Id = personId,
            Email = "email@email.com",
            FirstName = "fn",
            LastName = "ln",
        }, options => options.ComparingByMembers<Model.Person>());
    }
}
