using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using static CO.CDP.OrganisationInformation.Persistence.Organisation;
using ContactPoint = CO.CDP.OrganisationInformation.Persistence.Organisation.ContactPoint;
using Identifier = CO.CDP.OrganisationInformation.Persistence.Organisation.Identifier;

namespace DataSharing.Tests;
public static class DataSharingFactory
{
    public static CO.CDP.OrganisationInformation.Persistence.Forms.SharedConsent CreateMockSharedConsent()
    {
        return new CO.CDP.OrganisationInformation.Persistence.Forms.SharedConsent
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

    public static Organisation CreateMockOrganisation(bool withSupplierInfo = true)
    {
        return new Organisation
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            Name = "Test Organisation",
            Tenant = new Tenant { Guid = Guid.NewGuid(), Name = "TestTenant" },
            Addresses = new List<OrganisationAddress>
            {
                new OrganisationAddress
                {
                    Type = AddressType.Registered,
                    Address = new CO.CDP.OrganisationInformation.Persistence.Address
                    {
                        StreetAddress = "123 Test Street",
                        Locality = "Test Locality",
                        PostalCode = "12345",
                        CountryName = "Test Country",
                        Country = "TC"
                    }
                },
                    new OrganisationAddress
                {
                    Type = AddressType.Postal,
                    Address = new CO.CDP.OrganisationInformation.Persistence.Address
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
            Roles = new List<PartyRole> { PartyRole.Tenderer },
            SupplierInfo = withSupplierInfo
                ? new Organisation.SupplierInformation
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
                    Qualifications = new List<Qualification>
                    {
                            new Qualification
                            {
                                Guid = Guid.NewGuid(),
                                AwardedByPersonOrBodyName = "Certifying Authority",
                                DateAwarded = DateTimeOffset.UtcNow.AddYears(-2),
                                Name = "ISO 9001"
                            }
                    },
                    TradeAssurances = new List<TradeAssurance>
                    {
                            new TradeAssurance
                            {
                                Guid = Guid.NewGuid(),
                                AwardedByPersonOrBodyName = "Trade Assurance Authority",
                                ReferenceNumber = "TA123456",
                                DateAwarded = DateTimeOffset.UtcNow.AddYears(-1)
                            }
                    },
                    LegalForm = new Organisation.LegalForm
                    {
                        RegisteredUnderAct2006 = true,
                        RegisteredLegalForm = "Private Limited",
                        LawRegistered = "UK",
                        RegistrationDate = DateTimeOffset.UtcNow.AddYears(-10)
                    }
                }
                : null,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };
    }


    public static BasicInformation CreateMockBasicInformation()
    {
        return new BasicInformation
        {
            SupplierType = SupplierType.Organisation,
            RegisteredAddress = new CO.CDP.OrganisationInformation.Address
            {
                StreetAddress = "123 Test Street",
                Locality = "Test Locality",
                Region = "Test Region",
                PostalCode = "12345",
                CountryName = "Test Country",
                Country = "TC",
                Type = AddressType.Registered
            },
            PostalAddress = new CO.CDP.OrganisationInformation.Address
            {
                StreetAddress = "456 Postal Street",
                Locality = "Postal Locality",
                Region = "Postal Region",
                PostalCode = "67890",
                CountryName = "Postal Country",
                Country = "PC",
                Type = AddressType.Postal
            },
            VatNumber = "VAT123456",
            WebsiteAddress = "http://example.com",
            EmailAddress = "test@example.com",
            OrganisationType = OrganisationType.Supplier,
            Qualifications = new List<BasicQualification>
        {
            new BasicQualification
            {
                Guid = Guid.NewGuid(),
                AwardedByPersonOrBodyName = "Certifying Authority",
                DateAwarded = DateTimeOffset.UtcNow.AddYears(-2),
                Name = "ISO 9001"
            }
        },
            TradeAssurances = new List<BasicTradeAssurance>
        {
            new BasicTradeAssurance
            {
                Guid = Guid.NewGuid(),
                AwardedByPersonOrBodyName = "Trade Assurance Authority",
                ReferenceNumber = "TA123456",
                DateAwarded = DateTimeOffset.UtcNow.AddYears(-1)
            }
        },
            LegalForm = new BasicLegalForm
            {
                RegisteredLegalForm = "Private Limited",
                LawRegistered = "UK",
                RegistrationDate = DateTimeOffset.UtcNow.AddYears(-10)
            }
        };
    }
}
