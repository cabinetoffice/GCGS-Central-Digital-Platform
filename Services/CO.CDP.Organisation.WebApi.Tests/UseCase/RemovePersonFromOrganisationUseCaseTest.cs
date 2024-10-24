using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;
public class RemovePersonFromOrganisationUseCaseTest
{
    private readonly Mock<IOrganisationRepository> _organisationRepository = new();
    private readonly Mock<IPersonRepository> _personRepository = new();
    private RemovePersonFromOrganisationUseCase UseCase => new(_organisationRepository.Object, _personRepository.Object);

    [Fact]
    public async Task Execute_SuccessfulRemoval_ReturnsTrue()
    {
        var person = new OrganisationInformation.Persistence.Person { Id = 1, Guid = Guid.NewGuid(), FirstName = "Tom", LastName = "Smith", Email = "js@biz.com" };
        var organisation = new OrganisationInformation.Persistence.Organisation
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            Name = "Acme",
            OrganisationPersons = [new OrganisationPerson { PersonId = 1, Person = person, Organisation = null }],
            Tenant = new Tenant { Guid = Guid.NewGuid(), Name = "A Tenant" }
        };

        var command = (organisation.Guid, new RemovePersonFromOrganisation { PersonId = person.Guid });
        _organisationRepository.Setup(r => r.Find(command.Item1)).ReturnsAsync(organisation);
        _personRepository.Setup(r => r.FindByOrganisation(command.Item1)).ReturnsAsync(new List<OrganisationInformation.Persistence.Person> { person });
        _organisationRepository.Setup(r => r.FindPersonWithTenant(person.Guid)).ReturnsAsync(person);

        var result = await UseCase.Execute(command);

        result.Should().BeTrue();
        _organisationRepository.Verify(r => r.Save(organisation), Times.Once);
    }
}