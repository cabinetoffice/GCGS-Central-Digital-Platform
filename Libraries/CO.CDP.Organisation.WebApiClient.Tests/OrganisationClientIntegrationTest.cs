using CO.CDP.GovUKNotify;
using CO.CDP.MQ;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
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
        var mockPublisher = new Mock<IPublisher>();
        TestWebApplicationFactory<Program> _factory = new(builder =>
        {
            builder.ConfigureInMemoryDbContext<OrganisationInformationContext>();
            builder.ConfigureFakePolicyEvaluator();
            builder.ConfigureLogging(testOutputHelper);

            builder.ConfigureServices((_, s) =>
            {
                s.AddScoped(_ => _govUKNotifyApiClientMock.Object);
                s.AddScoped(_ => mockPublisher.Object);
            });
        });

        _httpClient = _factory.CreateClient();
    }

    [Fact]
    public async Task ItTalksToTheOrganisationApi()
    {
        IOrganisationClient client = new OrganisationClient("https://localhost", _httpClient);

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
            additionalIdentifiers: additionalIdentifiers,
            addresses: [address],
            contactPoint: contactPoint,
            identifier: identifier,
            name: "New Organisation",
            type: OrganisationType.Organisation,
            roles: roles
        );

        Func<Task> act = async () => await client.CreateOrganisationAsync(newOrganisation);

        var exception = await act.Should().ThrowAsync<ApiException<ProblemDetails>>();
        exception.Which.StatusCode.Should().Be(404);
        exception.Which.Result.Detail.Should().Contain("Unknown person");
    }
}