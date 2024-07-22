using CO.CDP.MQ;
using CO.CDP.Organisation.WebApi.Events;
using CO.CDP.Organisation.WebApi.MQ;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Address = CO.CDP.OrganisationInformation.Persistence.Address;
using ContactPoint = CO.CDP.Organisation.WebApi.Events.ContactPoint;
using Identifier = CO.CDP.Organisation.WebApi.Events.Identifier;

namespace CO.CDP.Organisation.WebApi.Tests.MQ;

public class MqOrganisationRepositoryTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IOrganisationRepository> _decoratedRepository = new();
    private readonly Mock<IPublisher> _publisher = new();

    private MqOrganisationRepository Repository =>
        new(_decoratedRepository.Object, _publisher.Object, mapperFixture.Mapper);

    [Fact]
    public void ItDelegatesSaveToTheDecoratedRepository()
    {
        var organisation = GivenOrganisation(guid: Guid.NewGuid());

        Repository.Save(organisation);

        _decoratedRepository.Verify(r => r.Save(organisation), Times.Once);
    }

    [Fact]
    public void ItPublishesOrganisationRegisteredEventWhenNewOrganisationIsSaved()
    {
        var organisation = GivenOrganisation(
            id: default,
            guid: Guid.NewGuid(),
            name: "Acme",
            roles: [PartyRole.Buyer],
            identifiers:
            [
                GivenIdentifier(scheme: "GB-COH", identifierId: "123123123", legalName: "Acme", uri: null,
                    primary: true),
                GivenIdentifier(scheme: "VAT", identifierId: "98765432", legalName: "Acme",
                    uri: "https://localhost/98765432")
            ],
            contactPoints:
            [
                GivenContactPoint(name: "Main", email: "contact@example.com", url: "https://example.com",
                    telephone: "07925432234")
            ],
            addresses:
            [
                GivenOrganisationAddress(
                    type: AddressType.Registered,
                    streetAddress: "10 Green Lane",
                    locality: "London",
                    postalCode: "SW19 8AR",
                    countryName: "United Kingdom")
            ]
        );

        Repository.Save(organisation);

        _publisher.Verify(r => r.Publish(It.IsAny<OrganisationRegistered>()), Times.Once);
        _publisher.Invocations[0].Arguments[0].Should().BeEquivalentTo(new OrganisationRegistered
        {
            Id = organisation.Guid.ToString(),
            Name = "Acme",
            Roles = ["Buyer"],
            Identifier = new Identifier
            {
                Id = "123123123",
                Scheme = "GB-COH",
                LegalName = "Acme",
                Uri = null
            },
            AdditionalIdentifiers =
            [
                new()
                {
                    Id = "98765432",
                    Scheme = "VAT",
                    LegalName = "Acme",
                    Uri = "https://localhost/98765432"
                }
            ],
            ContactPoint = new ContactPoint
            {
                Name = "Main",
                Email = "contact@example.com",
                Url = "https://example.com",
                Telephone = "07925432234"
            },
            Addresses =
            [
                new()
                {
                    Type = "Registered",
                    StreetAddress = "10 Green Lane",
                    Locality = "London",
                    Region = "",
                    PostalCode = "SW19 8AR",
                    CountryName = "United Kingdom"
                }
            ]
        });
    }

    [Fact]
    public async Task ItDelegatesFindToTheDecoratedRepository()
    {
        var organisation = GivenOrganisation(guid: Guid.NewGuid());

        _decoratedRepository.Setup(r => r.Find(organisation.Guid)).ReturnsAsync(organisation);

        var result = await Repository.Find(organisation.Guid);

        result.Should().Be(organisation);
    }

    [Fact]
    public async Task ItDelegatesFindByNameToTheDecoratedRepository()
    {
        var organisation = GivenOrganisation(name: "Acme");

        _decoratedRepository.Setup(r => r.FindByName("Acme")).ReturnsAsync(organisation);

        var result = await Repository.FindByName("Acme");

        result.Should().Be(organisation);
    }

    [Fact]
    public async Task ItDelegatesFindByUserUrnToTheDecoratedRepository()
    {
        var organisation = GivenOrganisation();

        _decoratedRepository.Setup(r => r.FindByUserUrn("urn:2024:uk:123")).ReturnsAsync([organisation]);

        var result = await Repository.FindByUserUrn("urn:2024:uk:123");

        result.Should().Equal([organisation]);
    }

    [Fact]
    public async Task ItDelegatesFindByIdentifierToTheDecoratedRepository()
    {
        var organisation = GivenOrganisation();

        _decoratedRepository.Setup(r => r.FindByIdentifier("GB-COH", "945234323")).ReturnsAsync(organisation);

        var result = await Repository.FindByIdentifier("GB-COH", "945234323");

        result.Should().Be(organisation);
    }

    [Fact]
    public void ItDelegatesDisposeToTheDecoratedRepository()
    {
        Repository.Dispose();

        _decoratedRepository.Verify(r => r.Dispose(), Times.Once);
    }

    private OrganisationInformation.Persistence.Organisation GivenOrganisation(
        int id = default,
        Guid? guid = null,
        string? name = null,
        List<PartyRole>? roles = null,
        List<OrganisationInformation.Persistence.Organisation.Identifier>? identifiers = null,
        List<OrganisationInformation.Persistence.Organisation.ContactPoint>? contactPoints = null,
        List<OrganisationInformation.Persistence.Organisation.OrganisationAddress>? addresses = null
    )
    {
        var theGuid = guid ?? Guid.NewGuid();
        var organisation = new OrganisationInformation.Persistence.Organisation
        {
            Id = id,
            Guid = theGuid,
            Name = name ?? $"Organisation {theGuid}",
            Tenant = new Tenant
            {
                Guid = theGuid,
                Name = $"Tenant {theGuid}"
            },
            Identifiers = identifiers ??
            [
                new OrganisationInformation.Persistence.Organisation.Identifier
                {
                    Primary = true,
                    Scheme = "ISO9001",
                    IdentifierId = "1",
                    LegalName = "DefaultLegalName",
                    Uri = "https://default.org"
                },
                new OrganisationInformation.Persistence.Organisation.Identifier
                {
                    Primary = false,
                    Scheme = "ISO14001",
                    IdentifierId = "2",
                    LegalName = "AnotherLegalName",
                    Uri = "https://example.com"
                }
            ],
            Addresses = addresses ?? [],
            ContactPoints = contactPoints ?? [],
            Roles = roles ?? [PartyRole.Supplier],
            BuyerInfo = null,
            SupplierInfo = null
        };
        return organisation;
    }

    private static OrganisationInformation.Persistence.Organisation.Identifier GivenIdentifier(
        string scheme = "GB-COH",
        string identifierId = "cee5ca59b1ae",
        string legalName = "Acme LTD",
        bool primary = false,
        string? uri = "https://example.org"
    ) => new()
    {
        Primary = primary,
        Scheme = scheme,
        IdentifierId = identifierId,
        LegalName = legalName,
        Uri = uri
    };

    private static OrganisationInformation.Persistence.Organisation.ContactPoint GivenContactPoint(
        string? name, string? email, string? url, string? telephone)
    {
        return new OrganisationInformation.Persistence.Organisation.ContactPoint
        {
            Name = name, Email = email, Url = url, Telephone = telephone
        };
    }

    private static OrganisationInformation.Persistence.Organisation.OrganisationAddress GivenOrganisationAddress(
        AddressType type, string streetAddress, string locality, string postalCode, string countryName) =>
        new()
        {
            Type = type,
            Address = new Address
            {
                StreetAddress = streetAddress,
                Locality = locality,
                Region = "",
                PostalCode = postalCode,
                CountryName = countryName
            }
        };
}