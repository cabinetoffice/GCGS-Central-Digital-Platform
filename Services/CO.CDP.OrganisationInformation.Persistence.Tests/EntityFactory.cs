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
        string scheme = "ISO9001",
        string identifierId = "1",
        string legalName = "DefaultLegalName",
        string uri = "http://default.org",
        string streetAddress = "1234 Default St",
        string city = "Default Region",
        string postCode = "12345",
        string country = "Defaultland",
        string contactName = "Default Contact",
        string email = "contact@default.org",
        string telephone = "123-456-7890",
        string contactUri = "http://contact.default.org",
        List<PartyRole>? roles = null)
    {
        var theGuid = guid ?? Guid.NewGuid();
        var theName = name ?? $"Organisation {theGuid}";

        return new Organisation
        {
            Guid = theGuid,
            Name = theName,
            Tenant = tenant ?? GivenTenant(name: theName),
            Identifiers = [new Organisation.Identifier
            {
                Primary = true,
                Scheme = scheme,
                IdentifierId = identifierId,
                LegalName = legalName,
                Uri = uri
            },
                new Organisation.Identifier
                {
                    Primary = false,
                    Scheme = "ISO14001",
                    IdentifierId = "2",
                    LegalName = "AnotherLegalName",
                    Uri = "http://example.com"
                }],
            Addresses = {new OrganisationAddress
            {
                Type  = AddressType.Registered,
                Address = new Address{
                    StreetAddress = streetAddress,
                    StreetAddress2 = "",
                    Locality = city,
                    Region = "",
                    PostalCode = postCode,
                    CountryName = country
                }
            } },
            ContactPoint = new OrganisationContactPoint
            {
                Name = contactName,
                Email = email,
                Telephone = telephone,
                Url = contactUri
            },
            Roles = roles ?? [PartyRole.Buyer]
        };
    }
}