using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Logging;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Buyer.Hierarchy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Buyer.Hierarchy;

public class ChildOrganisationResultsPageTests
{
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly Mock<ILogger<ChildOrganisationResultsPage>> _mockLogger;
    private readonly ChildOrganisationResultsPage _model;

    public ChildOrganisationResultsPageTests()
    {
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _mockLogger = new Mock<ILogger<ChildOrganisationResultsPage>>();
        _model = new ChildOrganisationResultsPage(_mockOrganisationClient.Object, _mockLogger.Object);
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
    public async Task OnPost_WithInvalidModelState_ReturnsPageResult()
    {
        _model.ModelState.AddModelError("Test", "Test error");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_WithValidModelState_WithResults_WithSelectionMade_RedirectsToConfirmPage()
    {
        var id = Guid.NewGuid();
        var childId = Guid.NewGuid();
        const string query = "test query";
        var searchResults = new List<OrganisationSearchResult>
        {
            CreateTestSearchResult("Test Org", childId, "DUNS", "123456789")
        };

        _model.Id = id;
        _model.Query = query;
        _model.SelectedChildId = childId;

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(
                query, "buyer", 20, 0.3))
            .ReturnsAsync(searchResults);

        var result = await _model.OnPost();

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("ChildOrganisationConfirmPage");
        redirectResult.RouteValues.Should().ContainKey("Id");
        redirectResult.RouteValues["Id"].Should().Be(id);
        redirectResult.RouteValues.Should().ContainKey("ChildId");
        redirectResult.RouteValues["ChildId"].Should().Be(childId);
        redirectResult.RouteValues.Should().ContainKey("Query");
        redirectResult.RouteValues["Query"].Should().Be(query);
    }

    [Fact]
    public void Results_DefaultsToEmptyList()
    {
        var mockOrganisationClient = new Mock<IOrganisationClient>();
        var mockLogger = new Mock<ILogger<ChildOrganisationResultsPage>>();
        var model = new ChildOrganisationResultsPage(mockOrganisationClient.Object, mockLogger.Object);

        model.Results.Should().NotBeNull();
        model.Results.Should().BeEmpty();
    }

    [Fact]
    public void SelectedPponIdentifier_DefaultsToNull()
    {
        var mockOrganisationClient = new Mock<IOrganisationClient>();
        var mockLogger = new Mock<ILogger<ChildOrganisationResultsPage>>();
        var model = new ChildOrganisationResultsPage(mockOrganisationClient.Object, mockLogger.Object);

        model.SelectedPponIdentifier.Should().BeNull();
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

    [Theory]
    [InlineData("GB-PPON:12345", "GB-PPON:12345")]
    [InlineData("GB-PPON-12345", "GB-PPON:12345")]
    [InlineData("ABCD-1234-EFGH", "GB-PPON:ABCD-1234-EFGH")]
    public async Task OnGetAsync_WithPponQuery_CallsLookupOrganisationAsync(string query, string expectedIdentifier)
    {
        _model.Query = query;
        var organisationId = Guid.NewGuid();
        var organisation = new CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: [],
            addresses: [],
            contactPoint: new ContactPoint("a@b.com", "Contact", "123", new Uri("http://whatever")),
            id: organisationId,
            identifier: new Identifier(expectedIdentifier, "asd", "PPON", new Uri("http://whatever")),
            name: "Test Ppon Organisation",
            type: CDP.Organisation.WebApiClient.OrganisationType.Organisation,
            roles: [CDP.Organisation.WebApiClient.PartyRole.Supplier, CDP.Organisation.WebApiClient.PartyRole.Tenderer],
            details: new Details(approval: null, buyerInformation: null, pendingRoles: [],
                publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null)
        );

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(null, expectedIdentifier))
            .ReturnsAsync(organisation);

        await _model.OnGetAsync();

        _mockOrganisationClient.Verify(
            client => client.LookupOrganisationAsync(null, expectedIdentifier),
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
        result.Name.Should().Be("Test Ppon Organisation");
        result.OrganisationId.Should().Be(organisationId);
    }

    [Fact]
    public async Task OnGetAsync_WithPponQuery_WhenLookupReturnsNull_ReturnsEmptyResults()
    {
        _model.Query = "GB-PPON-12345";

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(null, "GB-PPON:12345"))
            .ReturnsAsync((CDP.Organisation.WebApiClient.Organisation)null);

        await _model.OnGetAsync();

        _mockOrganisationClient.Verify(
            client => client.LookupOrganisationAsync(null, "GB-PPON:12345"),
            Times.Once);
        _model.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task OnGetAsync_WhenSearchThrowsException_LogsCdpExceptionAndRedirectsToErrorPage()
    {
        _model.Query = "test query";
        const string errorCode = "SEARCH_ERROR";
        var exception = new Exception("Test exception");

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(
                _model.Query, "buyer", 20, 0.3))
            .ThrowsAsync(exception);

        var result = await _model.OnGetAsync();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Error occurred while searching for organisations"),
                It.Is<Exception>(e => e is CdpExceptionLogging && ((CdpExceptionLogging)e).ErrorCode == errorCode),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(1));

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectToPageResult.PageName.Should().Be("/Error");
    }

    [Fact]
    public async Task OnGetAsync_WithPponQuery_WhenLookupThrowsException_LogsCdpExceptionAndRedirectsToErrorPage()
    {
        _model.Query = "GB-PPON-12345";
        const string errorCode = "LOOKUP_ERROR";
        var exception = new Exception("Test exception");

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(
                null, "GB-PPON:12345"))
            .ThrowsAsync(exception);

        var result = await _model.OnGetAsync();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Error occurred while searching for organisations"),
                It.Is<Exception>(e => e is CdpExceptionLogging && ((CdpExceptionLogging)e).ErrorCode == errorCode),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectToPageResult.PageName.Should().Be("/Error");
    }

    [Fact]
    public async Task OnPost_WithValidModelState_WithResults_WithNoSelectionMade_ReturnsPageResultWithError()
    {
        var id = Guid.NewGuid();
        const string query = "test query";
        var searchResults = new List<OrganisationSearchResult>
        {
            CreateTestSearchResult("Test Org", Guid.NewGuid(), "PPON", "123456789")
        };

        _model.Id = id;
        _model.Query = query;

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(
                query, "buyer", 20, 0.3))
            .ReturnsAsync(searchResults);

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
        _model.ErrorMessage.Should().Be(StaticTextResource.Global_RadioField_SelectOptionError);
    }

    [Fact]
    public async Task OnPost_WithNoResults_StaysOnPageWithErrorMessage()
    {
        var id = Guid.NewGuid();
        const string query = "test query with no results";
        var childId = Guid.Empty;

        _model.Id = id;
        _model.Query = query;
        _model.SelectedChildId = childId;

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(
                query, "buyer", 20, 0.3))
            .ReturnsAsync(new List<OrganisationSearchResult>());

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
        _model.ErrorMessage.Should().Be(StaticTextResource.BuyerParentChildRelationship_ResultsPage_NoResults);
        _model.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task OnPost_WhenSearchOrganisation404ExceptionOccurs_StaysOnPageWithNoResultsErrorMessage()
    {
        var id = Guid.NewGuid();
        const string query = "test query that causes 404";
        var childId = Guid.Empty;

        _model.Id = id;
        _model.Query = query;
        _model.SelectedChildId = childId;

        var httpRequestException = new HttpRequestException(
            "Not Found",
            null,
            System.Net.HttpStatusCode.NotFound);

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(
                query, "buyer", 20, 0.3))
            .ThrowsAsync(httpRequestException);

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
        _model.ErrorMessage.Should().Be(StaticTextResource.BuyerParentChildRelationship_ResultsPage_NoResults);
        _model.Results.Should().BeEmpty();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPost_WhenSearchOrganisationNon404ExceptionOccurs_ThrowsAndCausesRedirectToErrorPage()
    {
        var id = Guid.NewGuid();
        const string query = "test query that causes server error";
        var childId = Guid.Empty;

        _model.Id = id;
        _model.Query = query;
        _model.SelectedChildId = childId;

        var httpRequestException = new HttpRequestException(
            "Internal Server Error",
            null,
            System.Net.HttpStatusCode.InternalServerError);

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(
                It.Is<string>(q => q == query),
                It.Is<string>(r => r == "buyer"),
                It.Is<int>(l => l == 20),
                It.Is<double>(t => t == 0.3)))
            .ThrowsAsync(httpRequestException);

        var result = await _model.OnPost();

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Error", redirectResult.PageName);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Error occurred while searching for organisations"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnPost_WithPponLookup_WithResults_WithNoSelectionMade_ReturnsPageResultWithError()
    {
        var id = Guid.NewGuid();
        const string query = "GB-PPON-12345";
        const string expectedIdentifier = "GB-PPON:12345";
        var organisationId = Guid.NewGuid();

        var organisation = new CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: [],
            addresses: [],
            contactPoint: new ContactPoint("test@example.com", "Contact Name", "123456789", new Uri("http://example.com")),
            id: organisationId,
            identifier: new Identifier(expectedIdentifier, "Legal Name", "PPON", new Uri("http://identifier.example")),
            name: "Test PPON Organisation",
            type: OrganisationType.Organisation,
            roles: [PartyRole.Buyer],
            details: new Details(
                approval: null,
                buyerInformation: null,
                pendingRoles: [],
                publicServiceMissionOrganization: null,
                scale: null,
                shelteredWorkshop: null,
                vcse: null)
        );

        _model.Id = id;
        _model.Query = query;

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(
                null, expectedIdentifier))
            .ReturnsAsync(organisation);

        _model.Results.Add(MapOrganisationToChildOrganisation(organisation));

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
        _model.ErrorMessage.Should().Be(StaticTextResource.Global_RadioField_SelectOptionError);
    }

    [Fact]
    public async Task OnPost_WithPponLookup_WithResults_WithSelectionMade_RedirectsToConfirmPage()
    {
        var id = Guid.NewGuid();
        var childId = Guid.NewGuid();
        const string query = "GB-PPON-12345";
        const string expectedIdentifier = "GB-PPON:12345";

        var organisation = new CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: [],
            addresses: [],
            contactPoint: new ContactPoint("test@example.com", "Contact Name", "123456789", new Uri("http://example.com")),
            id: childId,
            identifier: new Identifier(expectedIdentifier, "Legal Name", "PPON", new Uri("http://identifier.example")),
            name: "Test PPON Organisation",
            type: OrganisationType.Organisation,
            roles: [PartyRole.Buyer],
            details: new Details(
                approval: null,
                buyerInformation: null,
                pendingRoles: [],
                publicServiceMissionOrganization: null,
                scale: null,
                shelteredWorkshop: null,
                vcse: null)
    );

        _model.Id = id;
        _model.Query = query;
        _model.SelectedChildId = childId;

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(
                null, expectedIdentifier))
            .ReturnsAsync(organisation);

        _model.Results.Add(MapOrganisationToChildOrganisation(organisation));

        var result = await _model.OnPost();

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("ChildOrganisationConfirmPage");
        redirectResult.RouteValues.Should().ContainKey("Id");
        redirectResult.RouteValues["Id"].Should().Be(id);
        redirectResult.RouteValues.Should().ContainKey("ChildId");
        redirectResult.RouteValues["ChildId"].Should().Be(childId);
        redirectResult.RouteValues.Should().ContainKey("Query");
        redirectResult.RouteValues["Query"].Should().Be(query);
    }

    private ChildOrganisation MapOrganisationToChildOrganisation(CDP.Organisation.WebApiClient.Organisation organisation)
    {
        return new ChildOrganisation(
            name: organisation.Name,
            organisationId: organisation.Id,
            identifier: organisation.Identifier
        );
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
