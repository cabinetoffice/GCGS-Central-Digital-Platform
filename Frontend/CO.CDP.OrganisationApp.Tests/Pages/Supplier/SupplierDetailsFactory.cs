using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;
public static class SupplierDetailsFactory
{
    public static Mock<IOrganisationClient> CreateOrganisationClientMock()
    {
        return new Mock<IOrganisationClient>();
    }

    public static SupplierInformation CreateSupplierInformationClientModel(
        bool completedTradeAssurance = false,
        bool completedPostalAddress = false)
    {
        return new SupplierInformation(
            organisationName: "FakeOrg",
            supplierType: SupplierType.Organisation,
            operationTypes: [],
            completedRegAddress: true,
            completedPostalAddress: completedPostalAddress,
            completedVat: true,
            completedWebsiteAddress: false,
            completedEmailAddress: false,
            completedQualification: false,
            completedTradeAssurance: completedTradeAssurance,
            completedOperationType: false,
            completedLegalForm: false,
            tradeAssurances: [],
            legalForm: null,
            qualifications: []
        );
    }

    public static Organisation.WebApiClient.Organisation GivenOrganisationClientModel(Guid id)
    {
        return new Organisation.WebApiClient.Organisation(
            additionalIdentifiers: [new Identifier(id: "FakeVatId", legalName: "FakeOrg", scheme: "VAT", uri: null)],
            addresses:
            [
                new Address(countryName: "United Kingdom", locality: "London", postalCode: "L1", region: "South", streetAddress: "1 London Street", streetAddress2: "", type: AddressType.Registered),
                new Address(countryName: "France", locality: "Paris", postalCode: "F1", region: "North", streetAddress: "1 Paris Street", streetAddress2: "", type: AddressType.Postal)
            ],
            contactPoint: new ContactPoint(email: "test@test.com", faxNumber: null, name: "fakecontact", telephone: "0123456789", url: new Uri("https://test.com")),
            id: id,
            identifier: null,
            name: "Test Org",
            roles: [PartyRole.Supplier]
        );
    }
}