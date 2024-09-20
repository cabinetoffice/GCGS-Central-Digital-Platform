using static CO.CDP.OrganisationInformation.Persistence.ConnectedEntity;
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

    public static PersonInvite GivenPersonInvite(Guid guid, string email = "invite@example.com", Organisation? organisation = null, Tenant? tenant = null)
    {
        return new PersonInvite
        {
            Guid = guid,
            FirstName = "John",
            LastName = "Doe",
            Email = email,
            Organisation = organisation ?? GivenOrganisation(),
            Person = null,
            Scopes = new List<string> { "scope1", "scope2" },
            InviteSentOn = DateTimeOffset.UtcNow,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };
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
                    Scheme = "CDP-PPON",
                    IdentifierId = $"{theGuid}",
                    LegalName = "DefaultLegalName",
                    Uri = "https://default.org"
                },
                new Organisation.Identifier
                {
                    Primary = false,
                    Scheme = "GB-COH",
                    IdentifierId = Guid.NewGuid().ToString(),
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
                    Locality = "Default City",
                    Region = "",
                    PostalCode = "12345",
                    CountryName = "Defaultland",
                    Country = "AB"
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
                Locality = "London",
                Region = "",
                PostalCode = "SW19 8AR",
                CountryName = "United Kingdom",
                Country = "GB"
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
        Guid = Guid.NewGuid(),
        Name = name,
        AwardedByPersonOrBodyName = "Qualification Centre",
        DateAwarded = DateTimeOffset.Parse("2018-02-20T00:00:00Z")
    };

    public static TradeAssurance GivenSupplierTradeAssurance()
        => new()
        {
            Guid = Guid.NewGuid(),
            AwardedByPersonOrBodyName = "Assurance Body",
            ReferenceNumber = "QA-12333",
            DateAwarded = DateTimeOffset.Parse("2009-10-03T00:00:00Z")
        };

    public static LegalForm GivenSupplierLegalForm(
        string registeredLegalForm = "Limited company"
    )
        => new()
        {
            RegisteredUnderAct2006 = true,
            RegisteredLegalForm = registeredLegalForm,
            LawRegistered = "England and Wales",
            RegistrationDate = DateTimeOffset.Parse("2005-12-02T00:00:00Z")
        };

    public static ConnectedEntity GivenConnectedOrganisation(
               Organisation supplierOrganisation,
               string name = "Test Connected Organisation",
               ConnectedOrganisationCategory category = ConnectedOrganisationCategory.RegisteredCompany,
               Guid? organisationId = null
           )
    {
        var connectedOrganisation = new ConnectedEntity.ConnectedOrganisation
        {
            Id = 1,
            Category = (ConnectedEntity.ConnectedOrganisationCategory)category,
            Name = name,
            OrganisationId = organisationId,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };

        return new ConnectedEntity
        {
            Guid = Guid.NewGuid(),
            EntityType = (ConnectedEntity.ConnectedEntityType)ConnectedEntityType.Organisation,
            Organisation = connectedOrganisation,
            SupplierOrganisation = supplierOrganisation,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };
    }

    public static ConnectedEntity GivenConnectedIndividualTrust(
               Organisation supplierOrganisation,
               string firstName = "John",
               string lastName = "Doe",
               ConnectedPersonCategory category = ConnectedPersonCategory.PersonWithSignificantControl
           )
    {
        var individualTrust = new ConnectedEntity.ConnectedIndividualTrust
        {
            Id = 1,
            FirstName = firstName,
            LastName = lastName,
            Category = (ConnectedEntity.ConnectedPersonCategory)category,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };

        return new ConnectedEntity
        {
            Guid = Guid.NewGuid(),
            EntityType = (ConnectedEntity.ConnectedEntityType)ConnectedEntityType.Individual,
            IndividualOrTrust = individualTrust,
            SupplierOrganisation = supplierOrganisation,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };
    }


}