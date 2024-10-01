using CO.CDP.Organisation.WebApiClient;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;
public static class SupplierDetailsFactory
{
    public static Mock<IOrganisationClient> CreateOrganisationClientMock()
    {
        return new Mock<IOrganisationClient>();
    }

    public static SupplierInformation CreateSupplierInformationClientModel(
        bool completedPostalAddress = false,
        bool completedLegalForm = false,
        LegalForm? legalForm = null)
    {
        return new SupplierInformation(
            organisationName: "FakeOrg",
            supplierType: SupplierType.Organisation,
            operationTypes: new List<OperationType>() { OperationType.SmallorMediumSized, OperationType.NonGovernmental },
            completedRegAddress: true,
            completedPostalAddress: completedPostalAddress,
            completedVat: true,
            completedWebsiteAddress: false,
            completedEmailAddress: false,
            completedOperationType: false,
            completedLegalForm: completedLegalForm,
            completedConnectedPerson: false,
            legalForm: legalForm
        );
    }

    public static CO.CDP.Organisation.WebApiClient.Organisation GivenOrganisationClientModel(Guid id)
    {
        return new CO.CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: [new Identifier(id: "FakeVatId", legalName: "FakeOrg", scheme: "VAT", uri: null)],
            addresses:
            [
                new Address(countryName: "United Kingdom", country: "GB", locality: "London", postalCode: "L1", region: "South", streetAddress: "1 London Street", type: AddressType.Registered),
                new Address(countryName: "France", country: "FR", locality: "Paris", postalCode: "F1", region: "North", streetAddress: "1 Paris Street", type: AddressType.Postal)
            ],
            null,
            contactPoint: new ContactPoint(email: "test@test.com", name: "fakecontact", telephone: "0123456789", url: new Uri("https://test.com")),
            id: id,
            identifier: null,
            name: "Test Org",
            roles: [PartyRole.Supplier]
        );
    }
}