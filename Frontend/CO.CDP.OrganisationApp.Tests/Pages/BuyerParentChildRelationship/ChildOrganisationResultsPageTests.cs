using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.BuyerParentChildRelationship;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.BuyerParentChildRelationship;

public class ChildOrganisationResultsPageTests
{
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly ChildOrganisationResultsPage _model;

    public ChildOrganisationResultsPageTests()
    {
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _model = new ChildOrganisationResultsPage(_mockOrganisationClient.Object);
    }

    [Fact]
    public async Task OnGetAsync_SetsPropertiesFromQuery()
    {
        var id = Guid.NewGuid();
        const string query = "test";

        _model.Id = id;
        _model.Query = query;

        await _model.OnGetAsync();

        _model.Id.Should().Be(id);
        _model.Query.Should().Be(query);
    }

    [Fact]
    public async Task OnGetAsync_WithNonEmptyQuery_PopulatesResults()
    {
        const string query = "test query";
        _model.Query = query;

        var searchResults = new List<OrganisationSearchResult>
        {
            CreateTestSearchResult("Stark Industries", Guid.NewGuid(), "DUNS", "123456789"),
            CreateTestSearchResult("Wayne Enterprises", Guid.NewGuid(), "PPON", "987654321"),
            CreateTestSearchResult("Oscorp", Guid.NewGuid(), "DUNS", "456789123")
        };

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(
                query, "buyer", 20, 0.3))
            .ReturnsAsync(searchResults);

        await _model.OnGetAsync();

        _model.Results.Should().HaveCount(3);
        _model.Results.Should().Contain(o => o.Name == "Stark Industries");
        _model.Results.Should().Contain(o => o.Name == "Wayne Enterprises");
        _model.Results.Should().Contain(o => o.Name == "Oscorp");

        var starkIndustries = _model.Results.FirstOrDefault(o => o.Name == "Stark Industries");
        starkIndustries?.GetFormattedIdentifier().Should().Be("DUNS: 123456789");
    }

    [Fact]
    public async Task OnGetAsync_WithEmptyQuery_ReturnsEmptyResults()
    {
        _model.Query = string.Empty;

        await _model.OnGetAsync();

        _model.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task OnGetAsync_WithNullQuery_ReturnsEmptyResults()
    {
        _model.Query = null;

        await _model.OnGetAsync();

        _model.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task OnGetAsync_WithWhitespaceQuery_ReturnsEmptyResults()
    {
        _model.Query = "   ";

        await _model.OnGetAsync();

        _model.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task OnGetAsync_WhenSearchThrowsException_ReturnsEmptyResults()
    {
        _model.Query = "test query";

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(
                _model.Query, "buyer", 20, 0.3))
            .ThrowsAsync(new Exception("Test exception"));

        await _model.OnGetAsync();

        _model.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task OnGetAsync_WhenSearchReturnsNull_ReturnsEmptyResults()
    {
        _model.Query = "test query";

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(
                _model.Query, "buyer", 20, 0.3))
            .ReturnsAsync((List<OrganisationSearchResult>)null);

        await _model.OnGetAsync();

        _model.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task OnGetAsync_WhenSearchReturnsEmpty_ReturnsEmptyResults()
    {
        _model.Query = "test query";

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(
                _model.Query, "buyer", 20, 0.3))
            .ReturnsAsync(new List<OrganisationSearchResult>());

        await _model.OnGetAsync();

        _model.Results.Should().BeEmpty();
    }

    [Fact]
    public void OnPost_WithInvalidModelState_ReturnsPageResult()
    {
        _model.ModelState.AddModelError("Test", "Test error");

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_WithValidModelState_ReturnsPageResult()
    {
        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void Results_DefaultsToEmptyList()
    {
        var model = new ChildOrganisationResultsPage(_mockOrganisationClient.Object);

        model.Results.Should().NotBeNull();
        model.Results.Should().BeEmpty();
    }

    [Fact]
    public void SelectedOrganisationId_DefaultsToNull()
    {
        var model = new ChildOrganisationResultsPage(_mockOrganisationClient.Object);

        model.SelectedOrganisationId.Should().BeNull();
    }

    [Fact]
    public void MapSearchResultToChildOrganisation_MapsPropertiesCorrectly()
    {
        var testId = Guid.NewGuid();
        var searchResults = new List<OrganisationSearchResult>
        {
            CreateTestSearchResult("Test Org", testId, "DUNS", "123456789")
        };

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(
                "test", "buyer", 20, 0.3))
            .ReturnsAsync(searchResults);

        _model.Query = "test";

        _model.OnGetAsync().Wait();

        _model.Results.Should().HaveCount(1);
        var result = _model.Results.First();
        result.Name.Should().Be("Test Org");
        result.OrganisationId.Should().Be(testId);
        result.GetFormattedIdentifier().Should().Be("DUNS: 123456789");
    }

    [Fact]
    public async Task OnGetAsync_WithNumericOnlyQuery_DoesNotCallOrganisationSearchAndReturnsEmptyResults()
    {
        _model.Query = "12345";

        await _model.OnGetAsync();

        _mockOrganisationClient.Verify(
            client => client.SearchOrganisationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<double>()),
            Times.Never);
        _model.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task OnGetAsync_WithNumericOnlyQuery_CallsLookupOrganisationAsync()
    {
        _model.Query = "12345";
        var organisationId = Guid.NewGuid();
        var organisation = new CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: [],
            addresses: [],
            contactPoint: new ContactPoint("a@b.com", "Contact", "123", new Uri("http://whatever")),
            id: organisationId,
            identifier: new Identifier("asd", "asd", "asd", new Uri("http://whatever")),
            name: "Org name",
            type: CDP.Organisation.WebApiClient.OrganisationType.Organisation,
            roles: [CDP.Organisation.WebApiClient.PartyRole.Supplier, CDP.Organisation.WebApiClient.PartyRole.Tenderer],
            details: new Details(approval: null, buyerInformation: null, pendingRoles: [],
                publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null)
        );

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(
                null, "12345"))
            .ReturnsAsync(organisation);

        await _model.OnGetAsync();

        _mockOrganisationClient.Verify(
            client => client.LookupOrganisationAsync(
                null, "12345"),
            Times.Once);
        _mockOrganisationClient.Verify(
            client => client.SearchOrganisationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<double>()),
            Times.Never);

        _model.Results.Should().HaveCount(1);
        var result = _model.Results.First();
        result.Name.Should().Be("Test Numeric Organisation");
        result.OrganisationId.Should().Be(organisationId);
        result.GetFormattedIdentifier().Should().Be("DUNS: 12345");
    }

    [Fact]
    public async Task OnGetAsync_WithNumericOnlyQuery_WhenLookupReturnsNull_ReturnsEmptyResults()
    {
        _model.Query = "12345";

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(
                null, "12345"))
            .ReturnsAsync((CDP.Organisation.WebApiClient.Organisation)null);

        await _model.OnGetAsync();

        _mockOrganisationClient.Verify(
            client => client.LookupOrganisationAsync(
                null, "12345"),
            Times.Once);
        _model.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task OnGetAsync_WithNumericOnlyQuery_WhenLookupThrowsException_ReturnsEmptyResults()
    {
        _model.Query = "12345";

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(
                null, "12345"))
            .ThrowsAsync(new Exception("Test exception"));

        await _model.OnGetAsync();

        _mockOrganisationClient.Verify(
            client => client.LookupOrganisationAsync(
                null, "12345"),
            Times.Once);
        _model.Results.Should().BeEmpty();
    }

    private static OrganisationSearchResult CreateTestSearchResult(string name, Guid id, string scheme,
        string identifierId)
    {
        var identifier = new Identifier(
            scheme: scheme,
            id: identifierId,
            legalName: null,
            uri: null
        );

        return new OrganisationSearchResult(
            id: id,
            identifier: identifier,
            name: name,
            roles: new List<PartyRole> { PartyRole.Buyer },
            type: OrganisationType.Organisation
        );
    }
}