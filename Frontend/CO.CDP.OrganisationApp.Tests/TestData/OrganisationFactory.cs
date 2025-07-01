using CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Tests.TestData;

internal static class OrganisationFactory
{
    public static CO.CDP.Organisation.WebApiClient.Organisation CreateOrganisation(
        Guid? id = null,
        string? name = null,
        ICollection<PartyRole>? roles = null,
        Identifier? identifier = null,
        ICollection<Identifier>? additionalIdentifiers = null,
        ICollection<Address>? addresses = null,
        ContactPoint? contactPoint = null,
        Details? details = null,
        OrganisationType? type = null)
    {
        return new CO.CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: additionalIdentifiers ?? [],
            addresses: addresses ??
            [
                new Address(
                    type: AddressType.Postal,
                    streetAddress: "123 Test Street",
                    locality: "Test City",
                    region: "Test Region",
                    postalCode: "T1 2ST",
                    country: "GB",
                    countryName: "United Kingdom"
                )
            ],
            contactPoint: contactPoint ?? new ContactPoint(
                email: "test@example.com",
                name: "Test Contact",
                telephone: "1234567890",
                url: new Uri("https://example.com")
            ),
            id: id ?? Guid.NewGuid(),
            identifier: identifier ?? new Identifier(
                scheme: "GB-PPON",
                id: "AAAA-1111-AAAA",
                legalName: "Test Legal Name",
                uri: new Uri("https://find-and-update.company-information.service.gov.uk/company/12345678")
            ),
            name: name ?? "Test Organisation",
            type: type ?? OrganisationType.Organisation,
            roles: roles ?? [PartyRole.Buyer, PartyRole.Supplier],
            details: details ?? new Details(null, null, [], null, null, null, null)
        );
    }
}