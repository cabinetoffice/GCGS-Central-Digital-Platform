using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using FluentAssertions;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class RegisterOrganisationUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<Persistence.IOrganisationRepository> _repository = new();
    private readonly Mock<Persistence.IPersonRepository> _persons = new();
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
            Addresses = [new OrganisationAddress
            {
                Type = AddressType.Registered,
                StreetAddress = "1234 Example St",
                StreetAddress2 = "",
                Locality = "Example City",
                Region = "Test Region",
                PostalCode = "12345",
                CountryName = "Exampleland"
            }],
            ContactPoint = new OrganisationContactPoint
            {
                Name = "Contact Name",
                Email = "contact@example.org",
                Telephone = "123-456-7890",
                Url = "https://example.org/contact"
            },
            Roles = [PartyRole.Supplier]
        };

        var createdOrganisation = await UseCase.Execute(command);

        var expectedOrganisation = new Model.Organisation
        {
            Id = _generatedGuid,
            Name = "TheOrganisation",
            Identifier = command.Identifier.AsView(),
            AdditionalIdentifiers = command.AdditionalIdentifiers.AsView(),
            Addresses = command.Addresses.AsView(),
            ContactPoint = command.ContactPoint.AsView(),
            Roles = command.Roles
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
            Addresses = [new OrganisationAddress
            {
                Type = AddressType.Registered,
                StreetAddress = "1234 Example St",
                StreetAddress2 = "",
                Locality = "Example City",
                Region = "Test Region",
                PostalCode = "12345",
                CountryName = "Exampleland"
            }],
            ContactPoint = new OrganisationContactPoint
            {
                Name = "Contact Name",
                Email = "contact@example.org",
                Telephone = "123-456-7890",
                Url = "https://example.org/contact"
            },
            Roles = [PartyRole.Supplier]
        });

        _repository.Verify(r => r.Save(It.Is<Persistence.Organisation>(o =>
             o.Guid == _generatedGuid &&
             o.Name == "TheOrganisation"&&
             o.ContactPoint == new Persistence.Organisation.OrganisationContactPoint
             {
                 Name = "Contact Name",
                 Email = "contact@example.org",
                 Telephone = "123-456-7890",
                 Url = "https://example.org/contact"
             } &&
             o.Roles.SequenceEqual(new List<PartyRole> { PartyRole.Supplier })
         )), Times.Once);

        persistanceOrganisation.Should().NotBeNull();
        persistanceOrganisation.As<Persistence.Organisation>().Identifiers.Should().HaveCount(2);
        persistanceOrganisation.As<Persistence.Organisation>().Identifiers.First()
            .Should().BeEquivalentTo(new Persistence.Organisation.Identifier
            {
                Primary = true,
                Scheme = "ISO9001",
                IdentifierId = "1",
                LegalName = "OfficialOrganisationName"
            });

        persistanceOrganisation.As<Persistence.Organisation>().Identifiers.Last()
            .Should().BeEquivalentTo(new Persistence.Organisation.Identifier
            {
                Primary = false,
                Scheme = "ISO14001",
                IdentifierId = "2",
                LegalName = "AnotherOrganisationName"
            });

        persistanceOrganisation.As<Persistence.Organisation>().Addresses.Should().HaveCount(1);
        persistanceOrganisation.As<Persistence.Organisation>().Addresses.First()
            .Should().BeEquivalentTo(new Persistence.Organisation.OrganisationAddress
            {
                Type = AddressType.Registered,
                Address = new Persistence.Address
                {
                    StreetAddress = "1234 Example St",
                    StreetAddress2 = "",
                    Locality = "Example City",
                    Region = "Test Region",
                    PostalCode = "12345",
                    CountryName = "Exampleland"
                }
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
            o.OrganisationPersons.Count == 1 && o.OrganisationPersons.First().Person.Guid == person.Guid
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

    [Fact]
    public async Task ItInitialisesBuyerInformationWhenRegisteringBuyerOrganisation()
    {
        var person = GivenPersonExists(guid: Guid.NewGuid());
        var command = GivenRegisterOrganisationCommand(
            personId: person.Guid,
            roles: [PartyRole.Buyer]
        );

        await UseCase.Execute(command);

        _repository.Verify(r => r.Save(It.Is<Persistence.Organisation>(o =>
            o.BuyerInfo != null && o.SupplierInfo == null
        )), Times.Once);
    }

    [Fact]
    public async Task ItInitialisesSupplierInformationWhenRegisteringSupplierOrganisation()
    {
        var person = GivenPersonExists(guid: Guid.NewGuid());
        var command = GivenRegisterOrganisationCommand(
            personId: person.Guid,
            roles: [PartyRole.Supplier]
        );

        await UseCase.Execute(command);

        _repository.Verify(r => r.Save(It.Is<Persistence.Organisation>(o =>
            o.BuyerInfo == null && o.SupplierInfo != null
        )), Times.Once);
    }

    private static RegisterOrganisation GivenRegisterOrganisationCommand(
        string name = "TheOrganisation",
        Guid? personId = null,
        List<PartyRole>? roles = null
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
            Addresses = [new OrganisationAddress
            {
                Type = AddressType.Registered,
                StreetAddress = "1234 Example St",
                StreetAddress2 = "",
                Locality = "Example City",
                Region = "Test Region",
                PostalCode = "12345",
                CountryName = "Exampleland"
            }],
            ContactPoint = new OrganisationContactPoint
            {
                Name = "Contact Name",
                Email = "contact@example.org",
                Telephone = "123-456-7890",
                Url = "https://example.org/contact"
            },
            Roles = roles ?? [PartyRole.Supplier]
        };
    }

    private Persistence.Person GivenPersonExists(Guid guid)
    {
        Persistence.Person person = new Persistence.Person
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