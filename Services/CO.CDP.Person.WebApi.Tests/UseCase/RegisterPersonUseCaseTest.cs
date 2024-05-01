using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Person.WebApi.Model;
using CO.CDP.Person.WebApi.Tests.AutoMapper;
using CO.CDP.Person.WebApi.UseCase;
using FluentAssertions;
using Moq;

namespace CO.CDP.Person.WebApi.Tests.UseCase;

public class RegisterPersonUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IPersonRepository> _repository = new();
    private readonly Guid _generatedGuid = Guid.NewGuid();
    private RegisterPersonUseCase UseCase => new(_repository.Object, mapperFixture.Mapper, () => _generatedGuid);

    [Fact]
    public async Task ItReturnsTheRegisteredOrganisation()
    {
        var command = new RegisterPerson
        {
            Name = "ThePerson",
            Age = 40,
            Email = "jon@email.com"
        };

        var createdPerson = await UseCase.Execute(command);

        var expectedPerson = new Model.Person
        {
            Id = _generatedGuid,
            Name = "ThePerson",
            Email = "jon@email.com",
            Age = 40
        };

        createdPerson.Should().BeEquivalentTo(expectedPerson, options => options.ComparingByMembers<Model.Person>());
    }

    [Fact]
    public void ItSavesNewTenantInTheRepository()
    {
        UseCase.Execute(new RegisterPerson
        {
            Name = "ThePerson",
            Email = "jon@email.com",
            Age = 40
        });

        _repository.Verify(r => r.Save(It.Is<OrganisationInformation.Persistence.Person>(o =>
             o.Guid == _generatedGuid &&
             o.Name == "ThePerson" &&
             o.Email == "jon@email.com"
         )), Times.Once);
    }
}