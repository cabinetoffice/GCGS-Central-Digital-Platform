using CO.CDP.GovUKNotify;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.TestKit.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

namespace CO.CDP.Organisation.WebApiClient.Tests;

public class OrganisationClientIntegrationTest
{
    private readonly HttpClient _httpClient;
    private readonly Mock<IGovUKNotifyApiClient> _govUKNotifyApiClientMock = new();

    public OrganisationClientIntegrationTest(ITestOutputHelper testOutputHelper)
    {
        TestWebApplicationFactory<Program> _factory = new(builder =>
        {
            builder.ConfigureInMemoryDbContext<OrganisationInformationContext>();
            builder.ConfigureFakePolicyEvaluator();
            builder.ConfigureLogging(testOutputHelper);
            builder.ConfigureServices((_, s) =>
            {
                s.AddScoped(_ => _govUKNotifyApiClientMock.Object);
            });
        });

        _httpClient = _factory.CreateClient();
    }

    [Fact]
    public async Task ItTalksToTheOrganisationApi()
    {
        IOrganisationClient client = new OrganisationClient("https://localhost", _httpClient);

        var unknownPersonId = Guid.NewGuid();
        var identifier = new OrganisationIdentifier(
            scheme: "ISO9001",
            id: "1234567",
            legalName: "New Org Legal Name"
        );
        var address = new OrganisationAddress(
            type: AddressType.Registered,
            streetAddress: "1234 New St",
            locality: "New City",
            region: "W.Yorkshire",
            postalCode: "123456",
            countryName: "Newland",
            country: "GB"
        );
        var contactPoint = new OrganisationContactPoint(
            name: "Main Contact",
            email: "contact@neworg.com",
            telephone: "123-456-7890",
            url: "http://contact.neworg.com"
        );
        var additionalIdentifiers = new List<OrganisationIdentifier>
        {
            new(
                scheme: "ISO14001",
                id: "1234567",
                legalName: "Additional Legal Name"
            )
        };
        var roles = new List<PartyRole> { PartyRole.Supplier };
        var buyerInfo = new BuyerInformation(
            buyerType: "Buyer Type",
            devolvedRegulations: [DevolvedRegulation.NorthernIreland, DevolvedRegulation.Wales]);

        var newOrganisation = new NewOrganisation(
            personId: unknownPersonId,
            additionalIdentifiers: additionalIdentifiers,
            addresses: [address],
            contactPoint: contactPoint,
            identifier: identifier,
            name: "New Organisation",
            roles: roles
        );

        var exception = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(() => client.CreateOrganisationAsync(newOrganisation));
        Assert.Equal(404, exception.StatusCode);
        Assert.Contains("Unknown person", exception.Result.Detail);
    }
}