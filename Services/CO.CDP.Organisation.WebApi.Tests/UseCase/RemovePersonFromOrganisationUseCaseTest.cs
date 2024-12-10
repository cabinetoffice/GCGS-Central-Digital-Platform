using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Person = CO.CDP.OrganisationInformation.Persistence.Person;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class RemovePersonFromOrganisationUseCaseTest
{
    private readonly Mock<IOrganisationRepository> _organisationRepository = new();

    private RemovePersonFromOrganisationUseCase UseCase => new(_organisationRepository.Object);

    [Fact]
    public async Task Execute_OrganisationPersonAndPersonWithTenant_BothLinksAreRemoved()
    {
        var person = GivenPerson();
        var organisation = GivenOrganisation(organisationPerson: person, tenantPerson: person);

        var command = (organisation.Guid, new RemovePersonFromOrganisation { PersonId = person.Guid });
        _organisationRepository.Setup(r => r.FindIncludingPersons(command.Item1)).ReturnsAsync(organisation);

        var result = await UseCase.Execute(command);

        result.Should().BeTrue();
        _organisationRepository.Verify(
            r => r.Save(It.Is<OrganisationInformation.Persistence.Organisation>(o =>
                o.OrganisationPersons.Count == 0 && o.Tenant.Persons.Count == 0)), Times.Once);
    }

    [Fact]
    public async Task Execute_NoOrganisationPerson_TenantLinkIsRemoved()
    {
        var person = GivenPerson();
        var organisation = GivenOrganisation(tenantPerson: person);

        var command = (organisation.Guid, new RemovePersonFromOrganisation { PersonId = person.Guid });
        _organisationRepository.Setup(r => r.FindIncludingPersons(command.Item1)).ReturnsAsync(organisation);

        var result = await UseCase.Execute(command);

        result.Should().BeTrue();
        _organisationRepository.Verify(
            r => r.Save(It.Is<OrganisationInformation.Persistence.Organisation>(o =>
                o.OrganisationPersons.Count == 0 && o.Tenant.Persons.Count == 0)), Times.Once);
    }

    [Fact]
    public async Task Execute_NoPersonWithTenant_OrganisationPersonIsRemoved()
    {
        var person = GivenPerson();
        var organisation = GivenOrganisation(organisationPerson: person);

        var command = (organisation.Guid, new RemovePersonFromOrganisation { PersonId = person.Guid });
        _organisationRepository.Setup(r => r.FindIncludingPersons(command.Item1)).ReturnsAsync(organisation);

        var result = await UseCase.Execute(command);

        result.Should().BeTrue();
        _organisationRepository.Verify(
            r => r.Save(It.Is<OrganisationInformation.Persistence.Organisation>(o =>
                o.OrganisationPersons.Count == 0 && o.Tenant.Persons.Count == 0)), Times.Once);
    }

    [Fact]
    public async Task Execute_NoOrganisationPersonNoPersonWithTenant_NoLinksAreRemoved()
    {
        var person = GivenPerson();
        var organisation = GivenOrganisation();

        var command = (organisation.Guid, new RemovePersonFromOrganisation { PersonId = person.Guid });
        _organisationRepository.Setup(r => r.FindIncludingPersons(command.Item1)).ReturnsAsync(organisation);

        var result = await UseCase.Execute(command);

        result.Should().BeFalse();
        _organisationRepository.Verify(r => r.Save(organisation), Times.Never);
    }

    private static Person GivenPerson()
    {
        return new Person
            { Id = 1, Guid = Guid.NewGuid(), FirstName = "Tom", LastName = "Smith", Email = "js@biz.com" };
    }

    private static OrganisationInformation.Persistence.Organisation GivenOrganisation(
        Person? organisationPerson = null,
        Person? tenantPerson = null
    )
    {
        var organisation = new OrganisationInformation.Persistence.Organisation
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            Name = "Acme",
            Type = OrganisationInformation.OrganisationType.Organisation,
            OrganisationPersons = [],
            Tenant = new Tenant { Guid = Guid.NewGuid(), Name = "A Tenant" }
        };
        if (organisationPerson != null)
        {
            organisation.OrganisationPersons.Add(new OrganisationPerson
                { PersonId = organisationPerson.Id, Person = organisationPerson, Organisation = organisation });
        }

        if (tenantPerson != null)
        {
            organisation.Tenant.Persons.Add(tenantPerson);
        }

        return organisation;
    }
}