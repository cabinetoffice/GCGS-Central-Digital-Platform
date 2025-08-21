using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Logging;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Buyer.Hierarchy;
using CO.CDP.OrganisationApp.Tests.TestData;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
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
        var mockAuthorizationService = new Mock<IAuthorizationService>();
        _model = new ChildOrganisationResultsPage(_mockOrganisationClient.Object, _mockLogger.Object);
        mockAuthorizationService.Setup(a => a.AuthorizeAsync(
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IAuthorizationRequirement[]>()))
            .ReturnsAsync(AuthorizationResult.Success());
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
                query, null, 20, 0.3, true))
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
                _model.Query, null, 20, 0.3, true))
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
                _model.Query, null, 20, 0.3, true))
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
                _model.Query, null, 20, 0.3, true))
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
                query, null, 20, 0.3, true))
            .ReturnsAsync(searchResults);

        await _model.OnGetAsync();

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
                "test", null, 20, 0.3, true))
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
                It.IsAny<double>(),
                It.IsAny<bool>()),
            Times.Never);

        _model.Results.Should().HaveCount(1);
        var result = _model.Results.First();
        result.Name.Should().Be("Test Ppon Organisation");
        result.OrganisationId.Should().Be(organisationId);
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
                _model.Query, null, 20, 0.3, true))
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
                query, null, 20, 0.3, true))
            .ReturnsAsync(searchResults);

        _mockOrganisationClient.Setup(c => c.GetChildOrganisationsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<OrganisationSummary>());

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
        _model.ErrorMessage.Should().Be(StaticTextResource.Global_RadioField_SelectOptionError);
    }

    [Fact]
    public async Task OnPost_WithNoResults_StaysOnPageWithNoResultsFoundMessage()
    {
        var id = Guid.NewGuid();
        const string query = "test query with no results";
        var childId = Guid.Empty;

        _model.Id = id;
        _model.Query = query;
        _model.SelectedChildId = childId;

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(
                query, null, 20, 0.3, true))
            .ReturnsAsync(new List<OrganisationSearchResult>());

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
        _model.FeedbackMessage.Should().Be(StaticTextResource.BuyerParentChildRelationship_ResultsPage_NoResults);
        _model.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task OnPost_WhenSearchOrganisation404ExceptionOccurs_StaysOnPageWithNoResultsFoundMessage()
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
                query, null, 20, 0.3, true))
            .ThrowsAsync(apiException);

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
        _model.FeedbackMessage.Should().Be(StaticTextResource.BuyerParentChildRelationship_ResultsPage_NoResults);
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
                It.Is<string?>(r => r == null),
                It.Is<int>(l => l == 20),
                It.Is<double>(t => Math.Abs(t - 0.3) < Tolerance),
                It.Is<bool>(f => f == true)))
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
            pendingRoles: new List<PartyRole>(),
            roles: new List<PartyRole> { PartyRole.Buyer },
            type: OrganisationType.Organisation
        );
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
                query, null, 20, 0.3, true))
            .ReturnsAsync(searchResults);

        _mockOrganisationClient
            .Setup(client => client.GetChildOrganisationsAsync(parentId))
            .ReturnsAsync(new List<OrganisationSummary>());

        await _model.OnGetAsync();

        _model.Results.Should().HaveCount(1);
        _model.Results.First().Name.Should().Be("GB-PPON Organisation");
    }

    [Fact]
    public async Task OnGetAsync_FiltersOutParentOrganisation()
    {
        var parentId = Guid.NewGuid();
        var childId1 = Guid.NewGuid();
        var childId2 = Guid.NewGuid();
        var childId3 = Guid.NewGuid();
        const string query = "test query";

        var searchResults = new List<OrganisationSearchResult>
        {
            CreateTestSearchResult("Parent Org", parentId, "GB-PPON", "PARENT-1234"),
            CreateTestSearchResult("Child Org 1", childId1, "GB-PPON", "CHILD-5678"),
            CreateTestSearchResult("Child Org 2", childId2, "GB-PPON", "CHILD-9012"),
            CreateTestSearchResult("Child Org 3", childId3, "GB-PPON", "CHILD-3456"),
        };

        _model.Id = parentId;
        _model.Query = query;

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(
                query, null, 20, 0.3, true))
            .ReturnsAsync(searchResults);

        _mockOrganisationClient.Setup(c => c.GetChildOrganisationsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<OrganisationSummary>());

        await _model.OnGetAsync();

        _model.Results.Should().HaveCount(3);
        _model.Results.Should().NotContain(o => o.OrganisationId == parentId);
        _model.Results.Should().Contain(o => o.OrganisationId == childId1);
        _model.Results.Should().Contain(o => o.OrganisationId == childId2);
        _model.Results.Should().Contain(o => o.OrganisationId == childId3);

        var resultsList = _model.Results.OrderBy(r => r.Name).ToList();
        resultsList[0].Name.Should().Be("Child Org 1");
        resultsList[0].GetFormattedIdentifier().Should().Be("PPON: CHILD-5678");
        resultsList[1].Name.Should().Be("Child Org 2");
        resultsList[1].GetFormattedIdentifier().Should().Be("PPON: CHILD-9012");
        resultsList[2].Name.Should().Be("Child Org 3");
        resultsList[2].GetFormattedIdentifier().Should().Be("PPON: CHILD-3456");
    }

    [Fact]
    public async Task OnGet_SearchResults_OnlyReturnsOrganisationsWithBuyerRoleOrPendingBuyerRole()
    {
        var id = Guid.NewGuid();
        const string query = "test query";
        var allSearchResults = new List<OrganisationSearchResult>
        {
            // Should be included - has buyer role
            CreateTestSearchResultBasedOnRoles("Buyer Org", Guid.NewGuid(), "GB-PPON", "BORG-1234",
                roles: new List<PartyRole> { PartyRole.Buyer },
                pendingRoles: new List<PartyRole>()),

            // Should be included - has pending buyer role
            CreateTestSearchResultBasedOnRoles("Pending Buyer Org", Guid.NewGuid(), "GB-PPON", "PBORG-5678",
                roles: new List<PartyRole>(),
                pendingRoles: new List<PartyRole> { PartyRole.Buyer }),

            // Should NOT be included - no buyer role
            CreateTestSearchResultBasedOnRoles("Supplier Only", Guid.NewGuid(), "GB-PPON", "SORG-9012",
                roles: new List<PartyRole> { PartyRole.Supplier },
                pendingRoles: new List<PartyRole>()),

            // Should NOT be included - pending supplier only
            CreateTestSearchResultBasedOnRoles("Pending Supplier", Guid.NewGuid(), "GB-PPON", "PSORG-3456",
                roles: new List<PartyRole>(),
                pendingRoles: new List<PartyRole> { PartyRole.Supplier })
        };

        _model.Id = id;
        _model.Query = query;

        _mockOrganisationClient
            .Setup(client => client.SearchOrganisationAsync(
                query, null, 20, 0.3, true))
            .ReturnsAsync(allSearchResults);

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _model.Results.Should().HaveCount(2);
        _model.Results.Should().Contain(org =>
            org.Name == "Buyer Org");
        _model.Results.Should().Contain(org =>
            org.Name == "Pending Buyer Org");
    }

    private static OrganisationSearchResult CreateTestSearchResultBasedOnRoles(
        string name,
        Guid id,
        string scheme,
        string identifierId,
        List<PartyRole>? roles = null,
        List<PartyRole>? pendingRoles = null)
    {
        return new OrganisationSearchResult(
            id: id,
            name: name,
            type: OrganisationType.Organisation,
            identifier: new Identifier(
                scheme: scheme,
                id: identifierId,
                legalName: name,
                uri: new Uri($"https://test.com/{identifierId}")
            ),
            roles: roles ?? new List<PartyRole>(),
            pendingRoles: pendingRoles ?? new List<PartyRole>()
        );
    }
}
