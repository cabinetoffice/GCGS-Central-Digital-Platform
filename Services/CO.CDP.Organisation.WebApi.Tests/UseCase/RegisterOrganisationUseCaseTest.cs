using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.MQ;
using CO.CDP.Organisation.WebApi.Events;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.Tests.UseCase.Extensions;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class RegisterOrganisationUseCaseTest : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IIdentifierService> _identifierService = new();
    private readonly Mock<Persistence.IOrganisationRepository> _repository = new();
    private readonly Mock<Persistence.IPersonRepository> _persons = new();
    private readonly Mock<IPublisher> _publisher = new();
    private readonly Mock<IGovUKNotifyApiClient> _notifyApiClient = new();
    private readonly IConfiguration _mockConfiguration;    
    private readonly Mock<ILogger<RegisterOrganisationUseCase>> _logger = new();
    private readonly Guid _generatedGuid = Guid.NewGuid();
    private readonly AutoMapperFixture _mapperFixture;
    private RegisterOrganisationUseCase UseCase => new(
        _identifierService.Object,
        _repository.Object,
        _persons.Object,
        _notifyApiClient.Object,
        _publisher.Object,
        _mapperFixture.Mapper,
        _mockConfiguration,
        _logger.Object,
        () => _generatedGuid);

    public RegisterOrganisationUseCaseTest(AutoMapperFixture mapperFixture)
    {        
        _mapperFixture = mapperFixture;
        var inMemorySettings = new List<KeyValuePair<string, string?>>
        {
            new("GOVUKNotify:PersonInviteEmailTemplateId", "test-template-id"),
            new("OrganisationAppUrl", "http://baseurl/"),
            new("GOVUKNotify:RequestReviewApplicationEmailTemplateId", "template-id"),
            new("GOVUKNotify:SupportAdminEmailAddress", "admin@example.com"),
        };

        _mockConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task ItShouldLogErrorWhenConfigurationKeysAreMissing()
    {
        IConfiguration _configurationMock;
        var inMemorySettings = new List<KeyValuePair<string, string?>>
        {
            new("GOVUKNotify:PersonInviteEmailTemplateId", ""),
            new("OrganisationAppUrl", ""),
            new("GOVUKNotify:RequestReviewApplicationEmailTemplateId", ""),
            new("GOVUKNotify:SupportAdminEmailAddress", ""),
        };

        _configurationMock = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var person = GivenPersonExists(guid: Guid.NewGuid());

        var command = GivenRegisterOrganisationCommand(personId: person.Guid, roles: [PartyRole.Buyer]);


        _persons.Setup(p => p.Find(command.PersonId)).ReturnsAsync(person);

        var useCase = new RegisterOrganisationUseCase(
                            _identifierService.Object,
                            _repository.Object,
                            _persons.Object,
                            _notifyApiClient.Object,
                            _publisher.Object,
                            _mapperFixture.Mapper,
                            _configurationMock,
                            _logger.Object,
                            () => _generatedGuid
                        );

        await useCase.Execute(command);

        _logger.Verify(
    x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Missing configuration keys: OrganisationAppUrl, GOVUKNotify:RequestReviewApplicationEmailTemplateId, GOVUKNotify:SupportAdminEmailAddress")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)
        ), Times.Once);

        _notifyApiClient.Verify(g => g.SendEmail(It.IsAny<EmailNotificationRequest>()), Times.Never);
    }

    [Fact]
    public async Task ItReturnsTheRegisteredOrganisation()
    {
        var person = GivenPersonExists(guid: Guid.NewGuid());

        var command = GivenRegisterOrganisationCommand(personId: person.Guid);

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

        createdOrganisation.Should().BeEquivalentTo(expectedOrganisation,
            options => options.ComparingByMembers<Model.Organisation>());
    }

    [Fact]
    public async Task ItSavesNewOrganisationInTheRepositoryAndPublishEvent()
    {
        var person = GivenPersonExists(guid: Guid.NewGuid());

        Persistence.Organisation? persistanceOrganisation = null;

        _repository
            .Setup(x => x.Save(It.IsAny<Persistence.Organisation>()))
            .Callback<Persistence.Organisation>(b => persistanceOrganisation = b);

        await UseCase.Execute(GivenRegisterOrganisationCommand(personId: person.Guid));

        _repository.Verify(r => r.Save(It.Is<Persistence.Organisation>(o =>
            o.Guid == _generatedGuid &&
            o.Name == "TheOrganisation" &&
            o.Roles.SequenceEqual(new List<PartyRole> { PartyRole.Tenderer }) &&
            o.OrganisationPersons.First().Scopes.Count == 3 &&
            o.OrganisationPersons.First().Scopes[0] == "ADMIN" &&
            o.OrganisationPersons.First().Scopes[1] == "RESPONDER" &&
            o.OrganisationPersons.First().Scopes[2] == "EDITOR"
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


        persistanceOrganisation.As<Persistence.Organisation>().ContactPoints.First()
            .Should().BeEquivalentTo(new Persistence.Organisation.ContactPoint
            {
                Name = "Contact Name",
                Email = "contact@example.org",
                Telephone = "123-456-7890",
                Url = "https://example.org/contact"
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
                    Locality = "Example City",
                    Region = "Test Region",
                    PostalCode = "12345",
                    CountryName = "Exampleland",
                    Country = "AB"
                }
            });

        _publisher.Verify(x => x.Publish(It.IsAny<OrganisationRegistered>()), Times.Once);
    }

    [Fact]
    public async Task ItAssociatesTheTenantWithTheOrganisation()
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
    public async Task ItAssociatesTheTenantWithThePerson()
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
    public async Task ItAssociatesTheOrganisationWithThePerson()
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
    public async Task ItRejectsUnknownPersons()
    {
        var unknownPersonId = Guid.NewGuid();

        await UseCase
            .Invoking(u => u.Execute(GivenRegisterOrganisationCommand(
                name: "ACME",
                personId: unknownPersonId
            )))
            .Should()
            .ThrowAsync<UnknownPersonException>();

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
            roles: [PartyRole.Tenderer]
        );

        await UseCase.Execute(command);

        _repository.Verify(r => r.Save(It.Is<Persistence.Organisation>(o =>
            o.BuyerInfo == null && o.SupplierInfo != null
        )), Times.Once);
    }

    [Fact]
    public async Task ItPublishesOrganisationRegisteredEvent()
    {
        var person = GivenPersonExists();
        var command = GivenRegisterOrganisationCommand(name: "Acme Ltd", personId: person.Guid);

        await UseCase.Execute(command);

        _publisher.Verify(p => p.Publish(It.IsAny<OrganisationRegistered>()), Times.Once);
        _publisher.Invocations[0].Arguments[0].Should().BeEquivalentTo(new OrganisationRegistered
        {
            Id = _generatedGuid.ToString(),
            Name = "Acme Ltd",
            Identifier = command.Identifier.AsEventValue(),
            AdditionalIdentifiers = command.AdditionalIdentifiers.AsEventValue(),
            Addresses = command.Addresses.AsEventValue(),
            ContactPoint = command.ContactPoint.AsEventValue(),
            Roles = command.Roles.AsEventValue()
        });
    }

    [Fact]
    public async Task ItShouldSendEmailIfOrganisationIsBuyer()
    {
        var person = GivenPersonExists(guid: Guid.NewGuid());
        var roles = new List<PartyRole> { PartyRole.Buyer };
        var command = GivenRegisterOrganisationCommand(personId: person.Guid, roles: roles);

        _persons.Setup(x => x.Find(command.PersonId)).ReturnsAsync(person);

        Persistence.Organisation organisation = GetOrganisationEntity();

        Persistence.Organisation? persistanceOrganisation = null;
        _repository
            .Setup(x => x.Save(It.IsAny<Persistence.Organisation>()))
            .Callback<Persistence.Organisation>(b => persistanceOrganisation = b);

        var result = await UseCase.Execute(command);

        _notifyApiClient.Verify(x => x.SendEmail(It.Is<EmailNotificationRequest>(req =>
            req.EmailAddress == "admin@example.com" &&
            req.TemplateId == "template-id" &&
            req.Personalisation!["org_name"] == "TheOrganisation"
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
            Addresses =
            [
                new OrganisationAddress
                {
                    Type = AddressType.Registered,
                    StreetAddress = "1234 Example St",
                    Locality = "Example City",
                    Region = "Test Region",
                    PostalCode = "12345",
                    CountryName = "Exampleland",
                    Country = "AB"
                }
            ],
            ContactPoint = new OrganisationContactPoint
            {
                Name = "Contact Name",
                Email = "contact@example.org",
                Telephone = "123-456-7890",
                Url = "https://example.org/contact"
            },
            Roles = roles ?? [PartyRole.Tenderer]
        };
    }

    private Persistence.Person GivenPersonExists(Guid? guid = null)
    {
        var theGuid = guid ?? Guid.NewGuid();
        Persistence.Person person = new Persistence.Person
        {
            Id = 13,
            Guid = theGuid,
            FirstName = "Bob",
            LastName = "Smith",
            Email = "contact@example.com"
        };
        _persons.Setup(r => r.Find(theGuid)).ReturnsAsync(person);
        return person;
    }
    private Persistence.Organisation GetOrganisationEntity()
    {
        return new OrganisationInformation.Persistence.Organisation
        {
            Id = 1,
            Guid = _generatedGuid,
            Name = "TheOrganisation",
            Tenant = new Tenant
            {
                Id = 101,
                Guid = Guid.NewGuid(),
                Name = "Tenant 101"
            },
            Identifiers = [new OrganisationInformation.Persistence.Organisation.Identifier
            {
                Primary = true,
                IdentifierId = "123456",
                Scheme = "Scheme1",
                LegalName = "Legal Name",
                Uri = "https://example.com"
            },
                new OrganisationInformation.Persistence.Organisation.Identifier
                {
                    Primary = false,
                    IdentifierId = "123456",
                    Scheme = "Scheme2",
                    LegalName = "Another Legal Name",
                    Uri = "https://another-example.com"
                }],
            Addresses = {new OrganisationInformation.Persistence.Organisation.OrganisationAddress
            {
                Type = AddressType.Registered,
                Address = new Persistence.Address
                {
                    StreetAddress = "1234 Test St",
                    Locality = "Test City",
                    PostalCode = "12345",
                    CountryName = "Testland",
                    Country = "AB",
                    Region = ""
                }
            }},
            ContactPoints = [new OrganisationInformation.Persistence.Organisation.ContactPoint
            {
                Name = "Contact Name",
                Email = "contact@test.org",
                Telephone = "123-456-7890",
                Url = "https://contact.test.org"
            }],
            Roles = [PartyRole.Buyer]
        };
    }
}