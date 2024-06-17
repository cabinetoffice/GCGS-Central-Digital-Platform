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
        string? email = null,
        string phone = "07925123123",
        Tenant? tenant = null,
        List<(Organisation, List<string>)>? organisationsWithScope = null
    )
    {
        var personGuid = guid ?? Guid.NewGuid();
        var person = new Person
        {
            Guid = personGuid,
            UserUrn = userUrn ?? $"urn:fdc:gov.uk:2022:{Guid.NewGuid()}",
            FirstName = firstname,
            LastName = lastname,
            Email = email ?? $"jon{personGuid}@example.com",
            Phone = phone
        };
        if (tenant != null)
        {
            person.Tenants.Add(tenant);
        }

        foreach (var organisationWithScope in organisationsWithScope ?? [])
        {
            person.PersonOrganisations.Add(
                new OrganisationPerson
                {
                    Person = person,
                    Organisation = organisationWithScope.Item1,
                    Scopes = organisationWithScope.Item2
                }
            );
        }

        return person;
    }

    public static Organisation GivenOrganisation(
        Guid? guid = null,
        Tenant? tenant = null,
        string? name = null,
        List<Organisation.Identifier>? identifiers = null,
        List<OrganisationAddress>? addresses = null,
        Organisation.ContactPoint? contactPoint = null,
        List<PartyRole>? roles = null,
        List<(Person, List<string>)>? personsWithScope = null,
        BuyerInformation? buyerInformation = null,
        SupplierInformation? supplierInformation = null
    )
    {
        var theGuid = guid ?? Guid.NewGuid();
        var theName = name ?? $"Organisation {theGuid}";
        var organisation = new Organisation
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
                Type = AddressType.Registered,
                Address = new Address
                {
                    StreetAddress = "1234 Default St",
                    StreetAddress2 = "",
                    Locality = "Default City",
                    Region = "",
                    PostalCode = "12345",
                    CountryName = "Defaultland"
                }
            }],
            ContactPoints = contactPoint == null ? [new Organisation.ContactPoint
            {
                Name = "Default Contact",
                Email = "contact@default.org",
                Telephone = "123-456-7890",
                Url = "https://contact.default.org"
            }] : [contactPoint],
            Roles = roles ?? [PartyRole.Buyer],
            BuyerInfo = buyerInformation,
            SupplierInfo = supplierInformation
        };
        foreach (var personWithScope in personsWithScope ?? [])
        {
            organisation.OrganisationPersons.Add(
                new OrganisationPerson
                {
                    Person = personWithScope.Item1,
                    Organisation = organisation,
                    Scopes = personWithScope.Item2
                }
            );
        }
        return organisation;
    }

    public static Organisation.BuyerInformation GivenBuyerInformation(
        string? type = null
    ) => new()
    {
        BuyerType = type
    };

    public static SupplierInformation GivenSupplierInformation(
        SupplierType? type = null,
        List<Qualification>? qualifications = null,
        List<TradeAssurance>? tradeAssurances = null,
        LegalForm? legalForm = null,
        bool completedRegAddress = false,
        bool completedPostalAddress = false,
        bool completedVat = false,
        bool completedQualification = false,
        bool completedTradeAssurance = false,
        bool completedLegalForm = false
    ) => new()
    {
        SupplierType = type,
        Qualifications = qualifications ?? [],
        TradeAssurances = tradeAssurances ?? [],
        LegalForm = legalForm,
        CompletedRegAddress = completedRegAddress,
        CompletedPostalAddress = completedPostalAddress,
        CompletedVat = completedVat,
        CompletedQualification = completedQualification,
        CompletedTradeAssurance = completedTradeAssurance,
        CompletedLegalForm = completedLegalForm
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

    public static LegalForm GivenSupplierLegalForm(
        string registeredLegalForm = "Limited company"
    )
        => new()
        {
            RegisteredUnderAct2006 = "yes",
            RegisteredLegalForm = registeredLegalForm,
            LawRegistered = "England and Wales",
            RegistrationDate = DateTimeOffset.Parse("2005-12-02T00:00:00Z")
        };
}