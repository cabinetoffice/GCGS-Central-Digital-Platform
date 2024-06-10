using static CO.CDP.OrganisationInformation.Persistence.Organisation;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public static class EntityFactory
{
    public static Tenant GivenTenant(
        Guid? guid = null,
        string? name = null,
        Organisation? organisation = null,
        Person? person = null)
    {
        var theGuid = guid ?? Guid.NewGuid();
        var theName = name ?? $"Stefan {theGuid}";
        var tenant = new Tenant
        {
            Guid = theGuid,
            Name = theName
        };
        if (organisation != null)
        {
            tenant.Organisations.Add(organisation);
        }
        if (person != null)
        {
            tenant.Persons.Add(person);
        }

        return tenant;
    }

    public static Person GivenPerson(
        Guid? guid = null,
        string? userUrn = null,
        string firstname = "Jon",
        string lastname = "doe",
        string email = "jon@example.com",
        string phone = "07925123123",
        Tenant? tenant = null)
    {
        var person = new Person
        {
            Guid = guid ?? Guid.NewGuid(),
            UserUrn = userUrn ?? $"urn:fdc:gov.uk:2022:{Guid.NewGuid()}",
            FirstName = firstname,
            LastName = lastname,
            Email = email,
            Phone = phone
        };
        if (tenant != null)
        {
            person.Tenants.Add(tenant);
        }

        return person;
    }

    public static Organisation GivenOrganisation(
        Guid? guid = null,
        Tenant? tenant = null,
        string? name = null,
        List<Organisation.Identifier>? identifiers = null,
        List<OrganisationAddress>? addresses = null,
        string contactName = "Default Contact",
        string email = "contact@default.org",
        string telephone = "123-456-7890",
        string contactUri = "http://contact.default.org",
        List<PartyRole>? roles = null,
        BuyerInformation? buyerInformation = null,
        SupplierInformation? supplierInformation = null
    )
    {
        var theGuid = guid ?? Guid.NewGuid();
        var theName = name ?? $"Organisation {theGuid}";

        return new Organisation
        {
            Guid = theGuid,
            Name = theName,
            Tenant = tenant ?? GivenTenant(name: theName),
            Identifiers = identifiers ??
            [
                new Organisation.Identifier
                {
                    Primary = true,
                    Scheme = "ISO9001",
                    IdentifierId = "1",
                    LegalName = "DefaultLegalName",
                    Uri = "https://default.org"
                },
                new Organisation.Identifier
                {
                    Primary = false,
                    Scheme = "ISO14001",
                    IdentifierId = "2",
                    LegalName = "AnotherLegalName",
                    Uri = "http://example.com"
                }
            ],
            Addresses = addresses ?? [new OrganisationAddress
            {
                Type  = AddressType.Registered,
                Address = new Address{
                    StreetAddress = "1234 Default St",
                    StreetAddress2 = "",
                    Locality = "Default City",
                    Region = "",
                    PostalCode = "12345",
                    CountryName = "Defaultland"
                }
            }],
            ContactPoint = new OrganisationContactPoint
            {
                Name = contactName,
                Email = email,
                Telephone = telephone,
                Url = contactUri
            },
            Roles = roles ?? [PartyRole.Buyer],
            BuyerInfo = buyerInformation,
            SupplierInfo = supplierInformation
        };
    }

    public static BuyerInformation GivenBuyerInformation(
        string? type = null
    ) => new()
    {
        BuyerType = type
    };

    public static SupplierInformation GivenSupplierInformation(
        SupplierType? type = null,
        List<Qualification>? qualifications = null,
        List<TradeAssurance>? tradeAssurances = null
    ) => new()
    {
        SupplierType = type,
        Qualifications = qualifications ?? [],
        TradeAssurances = tradeAssurances ?? []
    };

    public static OrganisationAddress GivenOrganisationAddress(
        AddressType type
    ) =>
        new()
        {
            Type = type,
            Address = new Address
            {
                StreetAddress = "10 Green Lane",
                StreetAddress2 = "Blue House",
                Locality = "London",
                Region = "",
                PostalCode = "SW19 8AR",
                CountryName = "United Kingdom"
            }
        };

    public static Organisation.Identifier GivenIdentifier(
        string scheme = "GB-COH",
        string identifierId = "cee5ca59-b1ae-40e3-807a-adf8370799be",
        string legalName = "Acme LTD",
        bool primary = false,
        string uri = "https://example.org"
    ) => new()
    {
        Primary = primary,
        Scheme = scheme,
        IdentifierId = identifierId,
        LegalName = legalName,
        Uri = uri
    };

    public static Qualification GivenSupplierQualification(
        string name = "My Qualification"
    ) => new()
    {
        Name = name,
        AwardedByPersonOrBodyName = "Qualification Centre",
        DateAwarded = DateTimeOffset.Parse("2018-02-20T00:00:00Z")
    };

    public static TradeAssurance GivenSupplierTradeAssurance()
        => new()
        {
            AwardedByPersonOrBodyName = "Assurance Body",
            ReferenceNumber = "QA-12333",
            DateAwarded = DateTimeOffset.Parse("2009-10-03T00:00:00Z")
        };
}