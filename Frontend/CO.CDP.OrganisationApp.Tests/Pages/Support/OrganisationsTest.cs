using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Support;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Tests.Pages.Support;

public class OrganisationsModelTests
{
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly Mock<ISession> _mockSession;
    private readonly OrganisationsModel _organisationsModel;

    public OrganisationsModelTests()
    {
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _mockSession = new Mock<ISession>();
        _organisationsModel = new OrganisationsModel(_mockOrganisationClient.Object, _mockSession.Object);
    }

    [Fact]
    public async Task OnGet_SetsPropertiesCorrectly_ForBuyerType()
    {
        string type = "buyer";
        int pageNumber = 1;
        int totalOrganisations = 120;
        var organisations = new Tuple_2(new List<OrganisationDto>(), totalOrganisations);

        _mockOrganisationClient.Setup(client => client.GetAllOrganisationsAsync(type, type, null, 50, 0)).ReturnsAsync(organisations);

        var result = await _organisationsModel.OnGet(type, pageNumber);

        result.Should().BeOfType<PageResult>();
        _organisationsModel.Title.Should().Be("Buyer organisations");
        _organisationsModel.PageSize.Should().Be(50);
        _organisationsModel.CurrentPage.Should().Be(pageNumber);
        _organisationsModel.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task OnGet_SetsPropertiesCorrectly_ForSupplierType()
    {
        string type = "supplier";
        string role = "tenderer";
        int pageNumber = 2;
        int totalOrganisations = 200;
        var organisations = new Tuple_2(new List<OrganisationDto>(), totalOrganisations);
        _mockOrganisationClient.Setup(client => client.GetAllOrganisationsAsync(role, role, null, 50, 50)).ReturnsAsync(organisations);

        var result = await _organisationsModel.OnGet(type, pageNumber);

        result.Should().BeOfType<PageResult>();
        _organisationsModel.Title.Should().Be("Supplier organisations");
        _organisationsModel.PageSize.Should().Be(50);
        _organisationsModel.CurrentPage.Should().Be(pageNumber);
        _organisationsModel.TotalPages.Should().Be(4);
    }

    [Fact]
    public void CombineIdentifiers_ReturnsCorrectList()
    {
        var identifier = new Identifier(
            id: "12345678",
            legalName: "Mock Legal Name 1",
            scheme: "GB-COH",
            uri: new Uri("http://example.com/1")
        );

        var additionalIdentifiers = new List<Identifier>
        {
            new Identifier(
                id: "12345678",
                legalName: "Mock Legal Name 1",
                scheme: "GB-COH",
                uri: new Uri("http://example.com/1")
            ),
            new Identifier(
                id: "12345678",
                legalName: "Mock Legal Name 1",
                scheme: "GB-COH",
                uri: new Uri("http://example.com/1")
            )
        };

        var result = OrganisationsModel.CombineIdentifiers(identifier, additionalIdentifiers);

        result.Should().HaveCount(3);
        result.Should().Contain(identifier);
        result.Should().Contain(additionalIdentifiers[0]);
        result.Should().Contain(additionalIdentifiers[1]);
    }

    [Fact]
    public void CombineIdentifiers_HandlesNullIdentifier()
    {
        var additionalIdentifiers = new List<Identifier>
        {
            new Identifier(
                id: "12345678",
                legalName: "Mock Legal Name 1",
                scheme: "GB-COH",
                uri: new Uri("http://example.com/1")
            )
        };

        var result = OrganisationsModel.CombineIdentifiers(null, additionalIdentifiers);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be("12345678");
    }
}
