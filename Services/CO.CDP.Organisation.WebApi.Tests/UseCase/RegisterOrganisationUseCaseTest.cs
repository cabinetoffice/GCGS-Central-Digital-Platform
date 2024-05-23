using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using Persistence = CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class RegisterOrganisationUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IOrganisationRepository> _repository = new();
    private readonly Mock<IPersonRepository> _persons = new();
    private readonly Guid _generatedGuid = Guid.NewGuid();
    private RegisterOrganisationUseCase UseCase => new(_repository.Object, _persons.Object, mapperFixture.Mapper, () => _generatedGuid);

    [Fact]
    public async Task ItReturnsTheRegisteredOrganisation()
    {
        var person = GivenPersonExists(guid: Guid.NewGuid());

        var command = new RegisterOrganisation
        {
            Name = "TheOrganisation",
            PersonId = person.Guid,
            Identifier = new OrganisationIdentifier
            {
                Scheme = "ISO9001",
                Id = "1",
                LegalName = "OfficialOrganisationName"
            },
            AdditionalIdentifiers = new List<OrganisationIdentifier>
            {
                new OrganisationIdentifier
                {
                    Scheme = "ISO14001",
                    Id = "2",
                    LegalName = "AnotherOrganisationName"
                }
            },
            Address = new OrganisationAddress
            {
                StreetAddress = "1234 Example St",
                StreetAddress2 = "",
                Locality = "Example City",
                PostalCode = "12345",
                CountryName = "Exampleland"
            },
            ContactPoint = new OrganisationContactPoint
            {
                Name = "Contact Name",
                Email = "contact@example.org",
                Telephone = "123-456-7890",
                Url = "http://example.org/contact"
            },
            Types = new List<int> { 1 }
        };

        var createdOrganisation = await UseCase.Execute(command);

        var expectedOrganisation = new Model.Organisation
        {
            Id = _generatedGuid,
            Name = "TheOrganisation",
            Identifier = command.Identifier.AsView(),
            AdditionalIdentifiers = command.AdditionalIdentifiers.AsView(),
            Address = command.Address.AsView(),
            ContactPoint = command.ContactPoint.AsView(),
            Types = command.Types
        };

        createdOrganisation.Should().BeEquivalentTo(expectedOrganisation, options => options.ComparingByMembers<Model.Organisation>());
    }

    [Fact]
    public async void ItSavesNewOrganisationInTheRepository()
    {
        var person = GivenPersonExists(guid: Guid.NewGuid());

        Persistence.Organisation? persistanceOrganisation = null;
        _repository
            .Setup(x => x.Save(It.IsAny<Persistence.Organisation>()))
            .Callback<Persistence.Organisation>(b => persistanceOrganisation = b);

        await UseCase.Execute(new RegisterOrganisation
        {
            Name = "TheOrganisation",
            PersonId = person.Guid,
            Identifier = new OrganisationIdentifier
            {
                Scheme = "ISO9001",
                Id = "1",
                LegalName = "OfficialOrganisationName"
            },
            AdditionalIdentifiers = new List<OrganisationIdentifier>
            {
                new OrganisationIdentifier
                {
                    Scheme = "ISO14001",
                    Id = "2",
                    LegalName = "AnotherOrganisationName"
                }
            },
            Address = new OrganisationAddress
            {
                StreetAddress = "1234 Example St",
                StreetAddress2 = "",
                Locality = "Example City",
                PostalCode = "12345",
                CountryName = "Exampleland"
            },
            ContactPoint = new OrganisationContactPoint
            {
                Name = "Contact Name",
                Email = "contact@example.org",
                Telephone = "123-456-7890",
                Url = "http://example.org/contact"
            },
            Types = new List<int> { 1 }
        });

        _repository.Verify(r => r.Save(It.Is<Persistence.Organisation>(o =>
             o.Guid == _generatedGuid &&
             o.Name == "TheOrganisation" &&
             o.Address == new Persistence.Organisation.OrganisationAddress
             {
                 StreetAddress = "1234 Example St",
                 StreetAddress2 = "",
                 Locality = "Example City",
                 PostalCode = "12345",
                 CountryName = "Exampleland"
             } &&
             o.ContactPoint == new Persistence.Organisation.OrganisationContactPoint
             {
                 Name = "Contact Name",
                 Email = "contact@example.org",
                 Telephone = "123-456-7890",
                 Url = "http://example.org/contact"
             } &&
             o.Types.SequenceEqual(new List<int> { 1 })
         )), Times.Once);

        persistanceOrganisation.Should().NotBeNull();
        persistanceOrganisation.As<Persistence.Organisation>().Identifiers.Should().HaveCount(2);
        persistanceOrganisation.As<Persistence.Organisation>().Identifiers.First()
            .Should().BeEquivalentTo(new Persistence.Organisation.OrganisationIdentifier
            {
                Primary = true,
                Scheme = "ISO9001",
                IdentifierId = "1",
                LegalName = "OfficialOrganisationName"
            });

        persistanceOrganisation.As<Persistence.Organisation>().Identifiers.Last()
            .Should().BeEquivalentTo(new Persistence.Organisation.OrganisationIdentifier
            {
                Primary = false,
                Scheme = "ISO14001",
                IdentifierId = "2",
                LegalName = "AnotherOrganisationName"
            });
    }

    [Fact]
    public async void ItAssociatesTheTenantWithTheOrganisation()
    {
        var person = GivenPersonExists(guid: Guid.NewGuid());

        await UseCase.Execute(GivenRegisterOrganisationCommand(
            name: "ACME",
            personId: person.Guid
        ));

        _repository.Verify(r => r.Save(It.Is<Persistence.Organisation>(o =>
            o.Tenant.Name == "ACME"
        )), Times.Once);
    }

    [Fact]
    public async void ItAssociatesTheTenantWithThePerson()
    {
        var person = GivenPersonExists(guid: Guid.NewGuid());

        await UseCase.Execute(GivenRegisterOrganisationCommand(
            name: "ACME",
            personId: person.Guid
        ));

        _repository.Verify(r => r.Save(It.Is<Persistence.Organisation>(o =>
            o.Tenant.Persons.Count == 1 && o.Tenant.Persons.First().Guid == person.Guid
        )), Times.Once);
    }

    [Fact]
    public async void ItAssociatesTheOrganisationWithThePerson()
    {
        var person = GivenPersonExists(guid: Guid.NewGuid());

        await UseCase.Execute(GivenRegisterOrganisationCommand(
            name: "ACME",
            personId: person.Guid
        ));

        _repository.Verify(r => r.Save(It.Is<Persistence.Organisation>(o =>
            o.Persons.Count == 1 && o.Persons.First().Guid == person.Guid
        )), Times.Once);
    }

    [Fact]
    public async void ItRejectsUnknownPersons()
    {
        var unknownPersonId = Guid.NewGuid();

        await UseCase
            .Invoking(u => u.Execute(GivenRegisterOrganisationCommand(
                name: "ACME",
                personId: unknownPersonId
            )))
            .Should()
            .ThrowAsync<RegisterOrganisationUseCase.RegisterOrganisationException.UnknownPersonException>();

        _repository.Verify(
            r => r.Save(It.IsAny<Persistence.Organisation>()),
            Times.Never);
    }

    private static RegisterOrganisation GivenRegisterOrganisationCommand(
        string name = "TheOrganisation",
        Guid? personId = null
    )
    {
        return new RegisterOrganisation
        {
            Name = name,
            PersonId = personId ?? Guid.NewGuid(),
            Identifier = new OrganisationIdentifier
            {
                Scheme = "ISO9001",
                Id = "1",
                LegalName = "OfficialOrganisationName"
            },
            AdditionalIdentifiers = new List<OrganisationIdentifier>
            {
                new OrganisationIdentifier
                {
                    Scheme = "ISO14001",
                    Id = "2",
                    LegalName = "AnotherOrganisationName"
                }
            },
            Address = new OrganisationAddress
            {
                StreetAddress = "1234 Example St",
                StreetAddress2 = "",
                Locality = "Example City",
                PostalCode = "12345",
                CountryName = "Exampleland"
            },
            ContactPoint = new OrganisationContactPoint
            {
                Name = "Contact Name",
                Email = "contact@example.org",
                Telephone = "123-456-7890",
                Url = "http://example.org/contact"
            },
            Types = new List<int> { 1 }
        };
    }

    private Person GivenPersonExists(Guid guid)
    {
        Person person = new Person
        {
            Id = 13,
            Guid = guid,
            FirstName = "Bob",
            LastName = "Smith",
            Email = "contact@example.com"
        };
        _persons.Setup(r => r.Find(guid)).ReturnsAsync(person);
        return person;
    }
}