using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Logging;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Buyer.Hierarchy;
using CO.CDP.OrganisationApp.Tests.TestData;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Buyer.Hierarchy;

public class ChildOrganisationResultsPageTests
{
    private const double Tolerance = 1e-6;
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
            CreateTestSearchResult("Stark Industries", Guid.NewGuid(), "GB-PPON", "STIN-1234-ABCD"),
            CreateTestSearchResult("Wayne Enterprises", Guid.NewGuid(), "GB-PPON", "WAYN-9876-EFGH"),
            CreateTestSearchResult("Oscorp", Guid.NewGuid(), "GB-PPON", "OSCO-4567-IJKL")
        };

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(
                query, "buyer", 20, 0.3))
            .ReturnsAsync(searchResults);

        _mockOrganisationClient.Setup(c => c.GetChildOrganisationsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<OrganisationSummary>());

        await _model.OnGetAsync();

        _model.Results.Should().HaveCount(3);
        _model.Results.Should().Contain(o => o.Name == "Stark Industries");
        _model.Results.Should().Contain(o => o.Name == "Wayne Enterprises");
        _model.Results.Should().Contain(o => o.Name == "Oscorp");

        var starkIndustries = _model.Results.FirstOrDefault(o => o.Name == "Stark Industries");
        starkIndustries?.GetFormattedIdentifier().Should().Be("PPON: STIN-1234-ABCD");
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
        _model.Query = null!;

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
            .ReturnsAsync((List<OrganisationSearchResult>)null!);

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
            CreateTestSearchResult("Test Org", childId, "GB-PPON", "TORG-1234-ABCD")
        };

        _model.Id = id;
        _model.Query = query;
        _model.SelectedChildId = childId;

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(
                query, "buyer", 20, 0.3))
            .ReturnsAsync(searchResults);

        _mockOrganisationClient.Setup(c => c.GetChildOrganisationsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<OrganisationSummary>());

        var result = await _model.OnPost();

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("ChildOrganisationConfirmPage");
        redirectResult.RouteValues.Should().NotBeNull();
        redirectResult.RouteValues.Should().ContainKey("Id");
        redirectResult.RouteValues?["Id"].Should().Be(id);
        redirectResult.RouteValues.Should().ContainKey("ChildId");
        redirectResult.RouteValues?["ChildId"].Should().Be(childId);
        redirectResult.RouteValues.Should().ContainKey("Query");
        redirectResult.RouteValues?["Query"].Should().Be(query);
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
    public async Task MapSearchResultToChildOrganisation_MapsPropertiesCorrectly()
    {
        var testId = Guid.NewGuid();
        var searchResults = new List<OrganisationSearchResult>
        {
            CreateTestSearchResult("Test Org", testId, "GB-PPON", "TORG-1234-BCDE")
        };

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(
                "test", "buyer", 20, 0.3))
            .ReturnsAsync(searchResults);

        _mockOrganisationClient.Setup(c => c.GetChildOrganisationsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<OrganisationSummary>());

        _model.Query = "test";

        await _model.OnGetAsync();

        _model.Results.Should().HaveCount(1);
        var result = _model.Results.First();
        result.Name.Should().Be("Test Org");
        result.OrganisationId.Should().Be(testId);
        result.GetFormattedIdentifier().Should().Be("PPON: TORG-1234-BCDE");
    }

    [Theory]
    [InlineData("GB-PPON:PMZV-7732-XXTT", "GB-PPON:PMZV-7732-XXTT")]
    [InlineData("GB-PPON-PMZV-7732-XXTT", "GB-PPON:PMZV-7732-XXTT")]
    [InlineData("PMZV-7732-XXTT", "GB-PPON:PMZV-7732-XXTT")]
    public async Task OnGetAsync_WithPponQuery_CallsLookupOrganisationAsync(string query, string expectedIdentifier)
    {
        _model.Query = query;
        var organisationId = Guid.NewGuid();
        var organisation = new CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: [],
            addresses: [],
            contactPoint: new ContactPoint("a@b.com", "Contact", "123", new Uri("http://whatever")),
            id: organisationId,
            identifier: new Identifier(scheme: "GB-PPON", id: expectedIdentifier.Substring("GB-PPON:".Length), legalName: "Test Ppon Organisation", uri: new Uri("http://whatever")),
            name: "Test Ppon Organisation",
            type: OrganisationType.Organisation,
            roles: [PartyRole.Buyer, PartyRole.Supplier, PartyRole.Tenderer],
            details: new Details(approval: null, buyerInformation: null, pendingRoles: [],
                publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null)
        );

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(null, expectedIdentifier))
            .ReturnsAsync(organisation);

        _mockOrganisationClient.Setup(c => c.GetChildOrganisationsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<OrganisationSummary>());

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
    public async Task OnGetAsync_WithPponQuery_FiltersOutOrganisationWithoutBuyerRole()
    {
        const string query = "GB-PPON-PMZV-7732-XXTT";
        const string expectedIdentifier = "GB-PPON:PMZV-7732-XXTT";
        _model.Query = query;
        var organisationId = Guid.NewGuid();
        var organisation = new CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: [],
            addresses: [],
            contactPoint: new ContactPoint("a@b.com", "Contact", "123", new Uri("http://whatever")),
            id: organisationId,
            identifier: new Identifier(scheme: "GB-PPON", id: "PMZV-7732-XXTT", legalName: "Test Ppon Organisation", uri: new Uri("http://whatever")),
            name: "Test Ppon Organisation",
            type: OrganisationType.Organisation,
            roles: [PartyRole.Supplier, PartyRole.Tenderer],
            details: new Details(approval: null, buyerInformation: null, pendingRoles: [],
                publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null)
        );

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(null, expectedIdentifier))
            .ReturnsAsync(organisation);

        _mockOrganisationClient.Setup(c => c.GetChildOrganisationsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<OrganisationSummary>());

        await _model.OnGetAsync();

        _model.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task OnGetAsync_WithPponQuery_WhenLookupReturnsNull_ReturnsEmptyResults()
    {
        _model.Query = "GB-PPON-PMZV-7732-XXTT";

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(null, "GB-PPON:PMZV-7732-XXTT"))
            .ReturnsAsync((CDP.Organisation.WebApiClient.Organisation)null!);

        await _model.OnGetAsync();

        _mockOrganisationClient.Verify(
            client => client.LookupOrganisationAsync(null, "GB-PPON:PMZV-7732-XXTT"),
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
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(1));

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectToPageResult.PageName.Should().Be("/Error");
    }

    [Fact]
    public async Task OnGetAsync_WithPponQuery_WhenLookupThrowsException_LogsCdpExceptionAndRedirectsToErrorPage()
    {
        _model.Query = "GB-PPON-PMZV-7732-XXTT";
        const string errorCode = "LOOKUP_ERROR";
        var exception = new Exception("Test exception");

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(
                null, "GB-PPON:PMZV-7732-XXTT"))
            .ThrowsAsync(exception);

        var result = await _model.OnGetAsync();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Error occurred while searching for organisations"),
                It.Is<Exception>(e => e is CdpExceptionLogging && ((CdpExceptionLogging)e).ErrorCode == errorCode),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
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
            CreateTestSearchResult("Test Org", Guid.NewGuid(), "GB-PPON", "TORG-1234-CDEF")
        };

        _model.Id = id;
        _model.Query = query;

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(
                query, "buyer", 20, 0.3))
            .ReturnsAsync(searchResults);

        _mockOrganisationClient.Setup(c => c.GetChildOrganisationsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<OrganisationSummary>());

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

        var apiException = new ApiException(
            "Not Found",
            (int)System.Net.HttpStatusCode.NotFound,
            "",
            new Dictionary<string, IEnumerable<string>>(),
            null);

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(
                query, "buyer", 20, 0.3))
            .ThrowsAsync(apiException);

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
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
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
                It.Is<double>(t => Math.Abs(t - 0.3) < Tolerance)))
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
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnPost_WithPponLookup_WithResults_WithNoSelectionMade_ReturnsPageResultWithError()
    {
        var id = Guid.NewGuid();
        const string query = "GB-PPON-PMZV-7732-XXTT";
        const string expectedIdentifier = "GB-PPON:PMZV-7732-XXTT";
        var organisationId = Guid.NewGuid();

        var organisation = new CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: [],
            addresses: [],
            contactPoint: new ContactPoint("test@example.com", "Contact Name", "123456789", new Uri("http://example.com")),
            id: organisationId,
            identifier: new Identifier(scheme: "GB-PPON", id: "PMZV-7732-XXTT", legalName: "Test PPON Organisation", uri: new Uri("http://identifier.example")),
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

        _mockOrganisationClient.Setup(c => c.GetChildOrganisationsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<OrganisationSummary>());

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
        const string query = "GB-PPON-PMZV-7732-XXTT";
        const string expectedIdentifier = "GB-PPON:PMZV-7732-XXTT";

        var organisation = new CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: [],
            addresses: [],
            contactPoint: new ContactPoint("test@example.com", "Contact Name", "123456789", new Uri("http://example.com")),
            id: childId,
            identifier: new Identifier(scheme: "GB-PPON", id: "PMZV-7732-XXTT", legalName: "Test PPON Organisation", uri: new Uri("http://identifier.example")),
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

        _mockOrganisationClient.Setup(c => c.GetChildOrganisationsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<OrganisationSummary>());

        _model.Results.Add(MapOrganisationToChildOrganisation(organisation));

        var result = await _model.OnPost();

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("ChildOrganisationConfirmPage");
        redirectResult.RouteValues.Should().NotBeNull();
        redirectResult.RouteValues.Should().ContainKey("Id");
        redirectResult.RouteValues?["Id"].Should().Be(id);
        redirectResult.RouteValues.Should().ContainKey("ChildId");
        redirectResult.RouteValues?["ChildId"].Should().Be(childId);
        redirectResult.RouteValues.Should().ContainKey("Query");
        redirectResult.RouteValues?["Query"].Should().Be(query);
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

    [Fact]
    public async Task OnGetAsync_WithNameSearch_FiltersOutParentOrganisation()
    {
        var parentId = Guid.NewGuid();
        const string query = "test organisation";

        _model.Id = parentId;
        _model.Query = query;

        var searchResults = new List<OrganisationSearchResult>
        {
            CreateTestSearchResult("Parent Organisation", parentId, "GB-PPON", "PORG-1234-ABCD"),
            CreateTestSearchResult("Child Organisation 1", Guid.NewGuid(), "GB-PPON", "CORG-9876-EFGH"),
            CreateTestSearchResult("Child Organisation 2", Guid.NewGuid(), "GB-PPON", "CORG-5554-IJKL")
        };

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(
                query, "buyer", 20, 0.3))
            .ReturnsAsync(searchResults);

        _mockOrganisationClient.Setup(c => c.GetChildOrganisationsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<OrganisationSummary>());

        await _model.OnGetAsync();

        _model.Results.Should().HaveCount(2);
        _model.Results.Should().NotContain(o => o.OrganisationId == parentId);
        _model.Results.Should().Contain(o => o.Name == "Child Organisation 1");
        _model.Results.Should().Contain(o => o.Name == "Child Organisation 2");
    }

    [Fact]
    public async Task OnGetAsync_WithPponSearch_FiltersOutParentOrganisation()
    {
        var parentId = Guid.NewGuid();
        const string query = "GB-PPON-PMZV-7732-XXTT";
        const string expectedIdentifier = "GB-PPON:PMZV-7732-XXTT";

        _model.Id = parentId;
        _model.Query = query;

        var organisation = new CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: [],
            addresses: [],
            contactPoint: new ContactPoint("test@example.com", "Contact Name", "123456789",
                new Uri("http://example.com")),
            id: parentId,
            identifier: new Identifier(scheme: "GB-PPON", id: "PMZV-7732-XXTT", legalName: "Parent Organisation", uri: new Uri("http://identifier.example")),
            name: "Parent Organisation",
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

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(null, expectedIdentifier))
            .ReturnsAsync(organisation);

        await _model.OnGetAsync();

        _model.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task OnGetAsync_FiltersOutAlreadyConnectedChildOrganisations()
    {
        var parentId = Guid.NewGuid();
        const string query = "test organisation";

        _model.Id = parentId;
        _model.Query = query;

        var connectedChildId = Guid.NewGuid();
        var searchResults = new List<CO.CDP.Organisation.WebApiClient.OrganisationSearchResult>
        {
            CreateTestSearchResult("Connected Child", connectedChildId, "GB-PPON", "PMZV-7732-XXTT"),
            CreateTestSearchResult("New Child", Guid.NewGuid(), "GB-PPON", "NEWC-9876-ABCD")
        };

        var childOrganisations = new List<CO.CDP.Organisation.WebApiClient.OrganisationSummary>
        {
            new(connectedChildId, "GB-PPON:PMZV-7732-XXTT", "Connected Child",
                new List<CO.CDP.Organisation.WebApiClient.PartyRole>
                    { CO.CDP.Organisation.WebApiClient.PartyRole.Buyer })
        };

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(
                query, "buyer", 20, 0.3))
            .ReturnsAsync(searchResults);

        _mockOrganisationClient.Setup(c => c.GetChildOrganisationsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<OrganisationSummary>());

        _mockOrganisationClient
            .Setup(client => client.GetChildOrganisationsAsync(parentId))
            .ReturnsAsync(childOrganisations);

        await _model.OnGetAsync();

        _model.Results.Should().HaveCount(1);
        _model.Results.Should().NotContain(o => o.OrganisationId == connectedChildId);
        _model.Results.First().Name.Should().Be("New Child");
    }

    [Fact]
    public async Task OnGetAsync_WithPponSearch_FiltersOutAlreadyConnectedChildOrganisations()
    {
        var parentId = Guid.NewGuid();
        const string query = "GB-PPON-PMZV-7732-XXTT";
        const string expectedIdentifier = "GB-PPON:PMZV-7732-XXTT";
        var connectedChildId = Guid.NewGuid();

        _model.Id = parentId;
        _model.Query = query;

        var lookedUpOrganisation = OrganisationFactory.CreateOrganisation(
            id: connectedChildId,
            name: "Connected Child",
            roles: new List<PartyRole> { PartyRole.Buyer },
            identifier: new Identifier(scheme: "GB-PPON", id: "PMZV-7732-XXTT", legalName: "Connected Child",
                uri: new Uri("https://example.com")));

        var childOrganisations = new List<OrganisationSummary>
        {
            new(connectedChildId, "GB-PPON:PMZV-7732-XXTT", "Connected Child", new List<PartyRole> { PartyRole.Buyer })
        };

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(null, expectedIdentifier))
            .ReturnsAsync(lookedUpOrganisation);

        _mockOrganisationClient.Setup(c => c.GetChildOrganisationsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<OrganisationSummary>());

        _mockOrganisationClient
            .Setup(client => client.GetChildOrganisationsAsync(parentId))
            .ReturnsAsync(childOrganisations);

        await _model.OnGetAsync();

        _model.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task OnGetAsync_WithPponSearch_FiltersOutNonGbPponSchemes()
    {
        var parentId = Guid.NewGuid();
        const string query = "GB-PPON-PMZV-7732-XXTT";
        const string expectedIdentifier = "GB-PPON:PMZV-7732-XXTT";
        var childId = Guid.NewGuid();

        _model.Id = parentId;
        _model.Query = query;

        var lookedUpOrganisation = OrganisationFactory.CreateOrganisation(
            id: childId,
            name: "Non GB-PPON Organisation",
            roles: new List<PartyRole> { PartyRole.Buyer },
            identifier: new Identifier(scheme: "NOT-GB-PPON", id: "PMZV-7732-XXTT", legalName: "Non GB-PPON Organisation",
                uri: new Uri("https://example.com")));

        _mockOrganisationClient
            .Setup(client => client.LookupOrganisationAsync(null, expectedIdentifier))
            .ReturnsAsync(lookedUpOrganisation);

        _mockOrganisationClient
            .Setup(client => client.GetChildOrganisationsAsync(parentId))
            .ReturnsAsync(new List<OrganisationSummary>());

        await _model.OnGetAsync();

        _model.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task OnGetAsync_WithNameSearch_FiltersOutNonGbPponSchemes()
    {
        var parentId = Guid.NewGuid();
        const string query = "test organisation";

        _model.Id = parentId;
        _model.Query = query;

        var searchResults = new List<OrganisationSearchResult>
        {
            CreateTestSearchResult("GB-PPON Organisation", Guid.NewGuid(), "GB-PPON", "PPON-1234-ABCD"),
            CreateTestSearchResult("Non GB-PPON Organisation", Guid.NewGuid(), "NOT-GB-PPON", "NON-PPON-5678-EFGH")
        };

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(
                query, "buyer", 20, 0.3))
            .ReturnsAsync(searchResults);

        _mockOrganisationClient
            .Setup(client => client.GetChildOrganisationsAsync(parentId))
            .ReturnsAsync(new List<OrganisationSummary>());

        await _model.OnGetAsync();

        _model.Results.Should().HaveCount(1);
        _model.Results.First().Name.Should().Be("GB-PPON Organisation");
    }
}
