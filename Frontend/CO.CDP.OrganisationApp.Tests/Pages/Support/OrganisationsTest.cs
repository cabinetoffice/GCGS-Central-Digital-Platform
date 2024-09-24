using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp;
using CO.CDP.OrganisationApp.Pages.Support;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Tests.Pages.Support;

public class OrganisationsModelTests
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly Mock<ISession> _sessionMock;
    private readonly OrganisationsModel _organisationsModel;

    public OrganisationsModelTests()
    {
        _organisationClientMock = new Mock<IOrganisationClient>();
        _sessionMock = new Mock<ISession>();
        _organisationsModel = new OrganisationsModel(_organisationClientMock.Object, _sessionMock.Object);
    }

    [Fact]
    public async Task OnGet_ValidType_SetsTitleAndOrganisations_ReturnsPageResult()
    {
        var type = "buyer";
        var orgs = new List<OrganisationExtended>
        {
            new OrganisationExtended(
                additionalIdentifiers: new List<Identifier>
                {
                    new Identifier(
                        id: "12345678",
                        legalName: "Mock Legal Name 1",
                        scheme: "GB-COH",
                        uri: new Uri("http://example.com/1")
                    )
                },
                addresses: new List<Address>
                {
                    new Address(
                        country: "GB",
                        countryName: "United Kingdom",
                        locality: "Mock Town",
                        postalCode: "MK1 1AB",
                        region: "Mockshire",
                        streetAddress: "123 Mock St",
                        type: AddressType.Postal
                    )
                },
                contactPoint: new ContactPoint(
                    name: "John Doe",
                    email: "john.doe@example.com",
                    telephone: "+441234567890",
                    url: new Uri("http://example.com/contact")
                ),
                details: null,
                id: Guid.NewGuid(),
                identifier: new Identifier(
                    id: "12345678",
                    legalName: "Mock Legal Name 1",
                    scheme: "GB-COH",
                    uri: new Uri("http://example.com/1")
                ),
                name: "Mock Organisation 1",
                roles: new List<PartyRole> { PartyRole.Buyer, PartyRole.Supplier }
            ),
            new OrganisationExtended(
                additionalIdentifiers: new List<Identifier>
                {
                    new Identifier(
                        id: "87654321",
                        legalName: "Mock Legal Name 2",
                        scheme: "GB-COH",
                        uri: new Uri("http://example.com/2")
                    )
                },
                addresses: new List<Address>
                {
                    new Address(
                        country: "GB",
                        countryName: "United Kingdom",
                        locality: "Testville",
                        postalCode: "TS2 2XY",
                        region: "Testshire",
                        streetAddress: "456 Test Lane",
                        type: AddressType.Postal
                    )
                },
                contactPoint: new ContactPoint(
                    name: "Jane Doe",
                    email: "jane.doe@example.com",
                    telephone: "+441234567892",
                    url: new Uri("http://example.com/contact2")
                ),
                details: null,
                id: Guid.NewGuid(),
                identifier: new Identifier(
                    id: "87654321",
                    legalName: "Mock Legal Name 2",
                    scheme: "GB-COH",
                    uri: new Uri("http://example.com/2")
                ),
                name: "Mock Organisation 2",
                roles: new List<PartyRole> { PartyRole.Buyer }
            )
        };

        _organisationClientMock.Setup(client => client.GetAllOrganisationsAsync(type, 1000, 0))
            .ReturnsAsync(orgs);

        var result = await _organisationsModel.OnGet(type);

        _organisationsModel.Type.Should().Be(type);
        _organisationsModel.Title.Should().Be("Buyer organisations");
        _organisationsModel.Organisations.Should().HaveCount(2);
        result.Should().BeOfType<PageResult>();

        _organisationClientMock.Verify(client => client.GetAllOrganisationsAsync(type, 1000, 0), Times.Once);
    }

    [Fact]
    public async Task OnGet_EmptyOrganisations_ReturnsPageResultWithEmptyList()
    {
        var type = "buyer";
        var orgs = new List<OrganisationExtended>();

        _organisationClientMock.Setup(client => client.GetAllOrganisationsAsync(type, 1000, 0))
            .ReturnsAsync(orgs);

        var result = await _organisationsModel.OnGet(type);

        _organisationsModel.Type.Should().Be(type);
        _organisationsModel.Title.Should().Be("Buyer organisations");
        _organisationsModel.Organisations.Should().BeEmpty();
        result.Should().BeOfType<PageResult>();

        _organisationClientMock.Verify(client => client.GetAllOrganisationsAsync(type, 1000, 0), Times.Once);
    }

    [Fact]
    public async Task OnGet_ClientThrowsException_PropagatesException()
    {
        var type = "supplier";
        _organisationClientMock.Setup(client => client.GetAllOrganisationsAsync(type, 1000, 0))
            .ThrowsAsync(new Exception("API error"));

        Func<Task> act = async () => await _organisationsModel.OnGet(type);

        await act.Should().ThrowAsync<Exception>().WithMessage("API error");

        _organisationClientMock.Verify(client => client.GetAllOrganisationsAsync(type, 1000, 0), Times.Once);
    }
}
