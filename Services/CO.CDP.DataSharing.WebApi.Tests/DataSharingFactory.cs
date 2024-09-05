using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using static CO.CDP.OrganisationInformation.Persistence.Organisation;
using Address = CO.CDP.OrganisationInformation.Persistence.Address;
using ContactPoint = CO.CDP.OrganisationInformation.Persistence.Organisation.ContactPoint;
using Identifier = CO.CDP.OrganisationInformation.Persistence.Organisation.Identifier;

namespace DataSharing.Tests;

public static class DataSharingFactory
{
    public static SharedConsent CreateMockSharedConsent()
    {
        return new SharedConsent
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            OrganisationId = 1,
            Organisation = new Organisation
            {
                Id = 1,
                Guid = Guid.NewGuid(),
                Name = "Test Organisation",
                Tenant = new Tenant { Guid = Guid.NewGuid(), Name = "TestTenant" },
                Addresses = new List<OrganisationAddress>(),
                ContactPoints = new List<ContactPoint>(),
                Identifiers = new List<Identifier>(),
                Roles = new List<PartyRole>(),
                CreatedOn = DateTimeOffset.UtcNow,
                UpdatedOn = DateTimeOffset.UtcNow
            },
            FormId = 1,
            Form = null!,
            AnswerSets = null!,
            SubmissionState = default,
            SubmittedAt = null,
            FormVersionId = string.Empty,
            ShareCode = "valid-sharecode",
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };
    }

    public static Organisation CreateMockOrganisation(
        SupplierInformation? supplierInformation = null
    )
    {
        return new Organisation
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            Name = "Test Organisation",
            Tenant = new Tenant { Guid = Guid.NewGuid(), Name = "TestTenant" },
            Addresses = new List<OrganisationAddress>
            {
                new()
                {
                    Type = AddressType.Registered,
                    Address = new Address
                    {
                        StreetAddress = "123 Test Street",
                        Locality = "Test Locality",
                        PostalCode = "12345",
                        CountryName = "Test Country",
                        Country = "TC"
                    }
                },
                new()
                {
                    Type = AddressType.Postal,
                    Address = new Address
                    {
                        StreetAddress = "456 Postal Street",
                        Locality = "Postal Locality",
                        PostalCode = "67890",
                        CountryName = "Postal Country",
                        Country = "PC"
                    }
                }
            },
            ContactPoints = new List<ContactPoint>
            {
                new ContactPoint
                {
                    Email = "test@example.com",
                    Url = "http://example.com"
                }
            },
            Identifiers = new List<Identifier>
            {
                new Identifier
                {
                    IdentifierId = "VAT123456",
                    Scheme = "VAT",
                    LegalName = "Test Legal Name",
                    Primary = true
                }
            },
            Roles = [PartyRole.Tenderer],
            SupplierInfo = supplierInformation ?? new SupplierInformation
            {
                SupplierType = SupplierType.Organisation,
                CompletedRegAddress = true,
                CompletedPostalAddress = true,
                CompletedVat = true,
                CompletedWebsiteAddress = true,
                CompletedEmailAddress = true,
                CompletedQualification = true,
                CompletedTradeAssurance = true,
                CompletedLegalForm = true,
                Qualifications =
                [
                    new()
                    {
                        Guid = Guid.NewGuid(),
                        AwardedByPersonOrBodyName = "Certifying Authority",
                        DateAwarded = DateTimeOffset.UtcNow.AddYears(-2),
                        Name = "ISO 9001"
                    }
                ],
                TradeAssurances =
                [
                    new TradeAssurance
                    {
                        Guid = Guid.NewGuid(),
                        AwardedByPersonOrBodyName = "Trade Assurance Authority",
                        ReferenceNumber = "TA123456",
                        DateAwarded = DateTimeOffset.UtcNow.AddYears(-1)
                    }
                ],
                LegalForm = new LegalForm
                {
                    RegisteredUnderAct2006 = true,
                    RegisteredLegalForm = "Private Limited",
                    LawRegistered = "UK",
                    RegistrationDate = DateTimeOffset.UtcNow.AddYears(-10)
                }
            },
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };
    }
}