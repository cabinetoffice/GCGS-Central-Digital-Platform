using CO.CDP.MQ;
using CO.CDP.Organisation.WebApi.Events;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OiOrganisation = CO.CDP.OrganisationInformation.Persistence.Organisation;
using OiOrganisationPerson = CO.CDP.OrganisationInformation.Persistence.OrganisationPerson;
using OiOrganisationRepository = CO.CDP.OrganisationInformation.Persistence.IOrganisationRepository;
using OiPerson = CO.CDP.OrganisationInformation.Persistence.Person;
using OiTenant = CO.CDP.OrganisationInformation.Persistence.Tenant;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class RemovePersonFromOrganisationUseCaseTest
{
    private readonly Mock<OiOrganisationRepository> _organisationRepository = new();
    private readonly Mock<IPublisher> _publisherMock = new();

    private RemovePersonFromOrganisationUseCase UseCase => new(
        _organisationRepository.Object,
        _publisherMock.Object,
        NullLogger<RemovePersonFromOrganisationUseCase>.Instance);

    [Fact]
    public async Task Execute_OrganisationPersonAndPersonWithTenant_BothLinksAreRemoved()
    {
        var person = GivenPerson();
        var organisation = GivenOrganisation(organisationPerson: person, tenantPerson: person);

        var command = (organisation.Guid, new RemovePersonFromOrganisation { PersonId = person.Guid });
        _organisationRepository.Setup(r => r.FindIncludingPersons(command.Item1)).ReturnsAsync(organisation);
        _organisationRepository.Setup(r =>
            r.SaveAsync(It.IsAny<OiOrganisation>(), It.IsAny<Func<OiOrganisation, Task>>()));

        var result = await UseCase.Execute(command);

        result.Should().BeTrue();
        organisation.OrganisationPersons.Should().BeEmpty();
        organisation.Tenant.Persons.Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_NoOrganisationPerson_TenantLinkIsRemoved()
    {
        var person = GivenPerson();
        var organisation = GivenOrganisation(tenantPerson: person);

        var command = (organisation.Guid, new RemovePersonFromOrganisation { PersonId = person.Guid });
        _organisationRepository.Setup(r => r.FindIncludingPersons(command.Item1)).ReturnsAsync(organisation);
        _organisationRepository.Setup(r =>
            r.SaveAsync(It.IsAny<OiOrganisation>(), It.IsAny<Func<OiOrganisation, Task>>()));

        var result = await UseCase.Execute(command);

        result.Should().BeTrue();
        organisation.Tenant.Persons.Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_NoPersonWithTenant_OrganisationPersonIsRemoved()
    {
        var person = GivenPerson();
        var organisation = GivenOrganisation(organisationPerson: person);

        var command = (organisation.Guid, new RemovePersonFromOrganisation { PersonId = person.Guid });
        _organisationRepository.Setup(r => r.FindIncludingPersons(command.Item1)).ReturnsAsync(organisation);
        _organisationRepository.Setup(r =>
            r.SaveAsync(It.IsAny<OiOrganisation>(), It.IsAny<Func<OiOrganisation, Task>>()));

        var result = await UseCase.Execute(command);

        result.Should().BeTrue();
        organisation.OrganisationPersons.Should().BeEmpty();
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
        _organisationRepository.Verify(
            r => r.SaveAsync(It.IsAny<OiOrganisation>(), It.IsAny<Func<OiOrganisation, Task>>()), Times.Never);
    }

    [Fact]
    public async Task Execute_ThrowsUnknownOrganisationException_WhenOrganisationNotFound()
    {
        var orgId = Guid.NewGuid();
        _organisationRepository.Setup(r => r.FindIncludingPersons(orgId)).ReturnsAsync((OiOrganisation?)null);

        var command = (orgId, new RemovePersonFromOrganisation { PersonId = Guid.NewGuid() });

        await UseCase.Invoking(u => u.Execute(command))
            .Should().ThrowAsync<UnknownOrganisationException>();
    }

    [Fact]
    public async Task Execute_PublishesPersonRemovedFromOrganisation_WhenPersonFound()
    {
        var orgId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var person = GivenPerson(personId);
        var organisation = GivenOrganisation(organisationPerson: person, guid: orgId);

        var command = (orgId, new RemovePersonFromOrganisation { PersonId = personId });
        _organisationRepository.Setup(r => r.FindIncludingPersons(orgId)).ReturnsAsync(organisation);
        _organisationRepository
            .Setup(r => r.SaveAsync(It.IsAny<OiOrganisation>(), It.IsAny<Func<OiOrganisation, Task>>()))
            .Returns<OiOrganisation, Func<OiOrganisation, Task>>(async (o, onSave) => await onSave(o));
        _publisherMock
            .Setup(p => p.Publish(It.IsAny<PersonRemovedFromOrganisation>()))
            .Returns(Task.CompletedTask);

        await UseCase.Execute(command);

        _publisherMock.Verify(p => p.Publish(It.Is<PersonRemovedFromOrganisation>(e =>
            e.OrganisationId == orgId.ToString() && e.PersonId == personId.ToString())), Times.Once);
    }

    private static OiPerson GivenPerson(Guid? guid = null) => new()
    {
        Id = 1,
        Guid = guid ?? Guid.NewGuid(),
        FirstName = "Tom",
        LastName = "Smith",
        Email = "js@biz.com",
        UserUrn = "urn:1234"
    };

    private static OiOrganisation GivenOrganisation(
        OiPerson? organisationPerson = null,
        OiPerson? tenantPerson = null,
        Guid? guid = null)
    {
        var organisation = new OiOrganisation
        {
            Id = 1,
            Guid = guid ?? Guid.NewGuid(),
            Name = "Acme",
            Type = OrganisationType.Organisation,
            OrganisationPersons = [],
            Tenant = new OiTenant { Guid = Guid.NewGuid(), Name = "A Tenant" }
        };
        if (organisationPerson != null)
            organisation.OrganisationPersons.Add(new OiOrganisationPerson
                { PersonId = organisationPerson.Id, Person = organisationPerson, Organisation = organisation });
        if (tenantPerson != null)
            organisation.Tenant.Persons.Add(tenantPerson);
        return organisation;
    }
}