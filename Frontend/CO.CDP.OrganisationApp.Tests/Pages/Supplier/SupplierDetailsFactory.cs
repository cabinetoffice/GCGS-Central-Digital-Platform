using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;
public static class SupplierDetailsFactory
{
    public static Mock<ISession> CreateSessionMock()
    {
        return new Mock<ISession>();
    }

    public static Mock<IOrganisationClient> CreateOrganisationClientMock()
    {
        return new Mock<IOrganisationClient>();
    }

    public static SupplierIndividualOrOrgModel GivenSupplierIndividualOrOrgModel(Mock<ISession> sessionMock, Mock<IOrganisationClient> organisationClientMock)
    {
        return new SupplierIndividualOrOrgModel(sessionMock.Object, organisationClientMock.Object);
    }

    public static SupplierInformation CreateSupplierInformationClientModel()
    {
        return new SupplierInformation(
            organisationName: "FakeOrg",
            supplierType: SupplierType.Organisation,
            operationTypes: null,
            completedRegAddress: true,
            completedPostalAddress: false,
            completedVat: true,
            completedWebsiteAddress: false,
            completedEmailAddress: false,
            completedQualification: false,
            completedTradeAssurance: false,
            completedOperationType: false,
            completedLegalForm: false,
            qualifications: null
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
            contactPoint: null,
            id: id,
            identifier: null,
            name: "Test Org",
            roles: [PartyRole.Supplier]
        );
    }
}