using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence.NonEfEntities;
using static CO.CDP.OrganisationInformation.Persistence.ConnectedEntity;

namespace CO.CDP.DataSharing.WebApi.Tests;

public static class DataSharingFactory
{
    public static SharedConsentNonEf CreateSharedConsent(
        string? shareCode = null,
        OrganisationNonEf? organisation = null
    )
    {
        var theOrganisation = organisation ?? CreateOrganisation();
        return new SharedConsentNonEf
        {
            Guid = Guid.NewGuid(),
            Organisation = theOrganisation,
            Form = null!,
            AnswerSets = [],
            SubmissionState = default,
            SubmittedAt = null,
            ShareCode = shareCode ?? "valid-sharecode"
        };
    }

    public static OrganisationNonEf CreateOrganisation(
        SupplierInformationNonEf? supplierInformation = null
    )
    {
        return new OrganisationNonEf
        {
            Guid = Guid.NewGuid(),
            Name = "Test Organisation",
            Type = OrganisationType.Organisation,
            Addresses = new List<AddressNonEf>
            {
                new()
                {
                    Type = AddressType.Registered,
                    StreetAddress = "123 Test Street",
                    Locality = "Test Locality",
                    PostalCode = "12345",
                    CountryName = "Test Country",
                    Country = "TC"
                },
                new()
                {
                    Type = AddressType.Postal,
                    StreetAddress = "456 Postal Street",
                    Locality = "Postal Locality",
                    PostalCode = "67890",
                    CountryName = "Postal Country",
                    Country = "PC"
                }
            },
            ContactPoints = new List<ContactPointNonEf>
            {
                new ContactPointNonEf
                {
                    Email = "test@example.com",
                    Url = "http://example.com"
                }
            },
            Identifiers = new List<IdentifierNonEf>
            {
                new IdentifierNonEf
                {
                    IdentifierId = "VAT123456",
                    Scheme = "VAT",
                    LegalName = "Test Legal Name",
                    Primary = true
                }
            },
            Roles = [PartyRole.Tenderer],
            SupplierInfo = supplierInformation ?? new SupplierInformationNonEf
            {
                SupplierType = SupplierType.Organisation,
                CompletedRegAddress = true,
                CompletedPostalAddress = true,
                CompletedVat = true,
                CompletedWebsiteAddress = true,
                CompletedEmailAddress = true,
                CompletedLegalForm = true,
                LegalForm = new LegalFormNonEf
                {
                    RegisteredUnderAct2006 = true,
                    RegisteredLegalForm = "Private Limited",
                    LawRegistered = "UK",
                    RegistrationDate = DateTimeOffset.UtcNow.AddYears(-10)
                }
            }
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
            Role = "Supplier",
            LegalForm = new BasicLegalForm
            {
                RegisteredLegalForm = "Private Limited",
                LawRegistered = "UK",
                RegistrationDate = DateTimeOffset.UtcNow.AddYears(-10)
            },
            OrganisationName = "Organisation Name"
        };
    }

    public static List<FormAnswerSetForPdf> CreateMockFormAnswerSetForPdfs()
    {
        return
            [
                new FormAnswerSetForPdf() {
                    QuestionAnswers = [
                        new Tuple<string, string>("Did this exclusion happen in the UK? ", "Yes"),
                        new Tuple<string, string>("Enter an email address ", "john.smith@acme.com")
                        ],
                    SectionName = "Exclusions",
                    SectionType = OrganisationInformation.Persistence.Forms.FormSectionType.Exclusions
                }
            ];
    }

    public static List<ConnectedPersonInformation> CreateMockConnectedPersonInformation()
    {
        return
        [
            new ConnectedPersonInformation(
                Guid.NewGuid(),
                "John",
                "Doe",
                "British",
                DateTimeOffset.Now.AddYears(-30),
                ConnectedPersonType.Individual,
                ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndiv,
                "UK",
                new List<ConnectedAddress>
                {
                    new ConnectedAddress(
                        "123 Main St",
                        "London",
                        "London",
                        "12345",
                        "UK",
                        AddressType.Registered
                    )
                },
                new List<string> { "ControlCondition1" },
                "12345",
                null,
                null,
                OrganisationInformation.ConnectedEntityType.Individual,
                ConnectedOrganisationCategory.RegisteredCompany
            )
        ];
    }
}