using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Organisation;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using AddressType = CO.CDP.Organisation.WebApiClient.AddressType;
using Identifier = CO.CDP.Organisation.WebApiClient.Identifier;
using OrganisationType = CO.CDP.Organisation.WebApiClient.OrganisationType;
using PartyRole = CO.CDP.Organisation.WebApiClient.PartyRole;

namespace CO.CDP.OrganisationApp.Tests.Pages.Organisation;

public class OrganisationPponSearchModelTest
{
    private const string DefaultSearchText = "Test Organisation";
    private const string DefaultSortOrder = "rel";
    private const int DefaultPageSize = 10;
    private const int DefaultPageNumber = 1;
    private static readonly Guid DefaultOrganisationId = Guid.NewGuid();
    private readonly string _defaultRefererUrl = $"https://example.com/organisation/{DefaultOrganisationId}/buyer/search";

    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly OrganisationPponSearchModel _testOrganisationPponSearchModel;
    private readonly Mock<ILogger<OrganisationPponSearchModel>> _mockLogger;

    public OrganisationPponSearchModelTest()
    {
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        Mock<ISession> mockSession = new Mock<ISession>();
        _mockLogger = new Mock<ILogger<OrganisationPponSearchModel>>();

        _testOrganisationPponSearchModel = new OrganisationPponSearchModel(_mockOrganisationClient.Object, mockSession.Object, _mockLogger.Object);

        var httpContext = new DefaultHttpContext();
        _testOrganisationPponSearchModel.PageContext = new PageContext { HttpContext = httpContext };
    }

    private void SetupRefererHeader()
    {
        _testOrganisationPponSearchModel.Request.Headers["Referer"] = _defaultRefererUrl;
    }

    private OrganisationSearchByPponResult CreateTestOrganisationResult(string name, List<PartyRole> roles)
    {
        return new OrganisationSearchByPponResult(
            new List<OrganisationAddress>
            {
                new OrganisationAddress(
                    country: "",
                    streetAddress: "123 Test Street",
                    locality: "Test City",
                    region: "Test Region",
                    postalCode: "AB12 3CD",
                    countryName: "United Kingdom",
                    type: AddressType.Postal)
            },
            Guid.NewGuid(),
            new List<Identifier>
            {
                new Identifier(
                    id: "123456",
                    scheme: "GB-PPON",
                    legalName: name,
                    uri: new Uri("https://www.example.com"))
            },
            name,
            roles,
            OrganisationType.Organisation
        );
    }

    private OrganisationSearchByPponResponse CreateTestResponse(List<OrganisationSearchByPponResult> results)
    {
        return new OrganisationSearchByPponResponse(results, results.Count);
    }

    private void SetupSuccessfulSearch(string searchText, string sortOrder, int pageSize, int skip,
        List<OrganisationSearchByPponResult> results)
    {
        var response = CreateTestResponse(results);

        _mockOrganisationClient.Setup(m => m.SearchByNameOrPponAsync(
                searchText,
                pageSize,
                skip,
                sortOrder))
            .ReturnsAsync(response);
    }

    [Fact]
    public async Task OnGet_WhenCalledWithoutSearchText_ShouldReturnPageWithEmptyResults()
    {
        SetupRouteData(DefaultOrganisationId);

        var result = await _testOrganisationPponSearchModel.OnGet(searchText: null);

        result.Should().BeOfType<PageResult>();
        _testOrganisationPponSearchModel.Organisations.Should().BeEmpty();
        _testOrganisationPponSearchModel.TotalOrganisations.Should().Be(0);
        _testOrganisationPponSearchModel.TotalPages.Should().Be(0);
        _testOrganisationPponSearchModel.Id.Should().Be(DefaultOrganisationId);
    }

    [Fact]
    public async Task OnGet_WhenCalledWithWhitespaceSearchText_ShouldReturnEmptyResults()
    {
        SetupRouteData(DefaultOrganisationId);

        var result = await _testOrganisationPponSearchModel.OnGet(DefaultPageNumber, "  ");

        result.Should().BeOfType<PageResult>();
        _testOrganisationPponSearchModel.Organisations.Should().BeEmpty();
        _testOrganisationPponSearchModel.TotalOrganisations.Should().Be(0);
        _testOrganisationPponSearchModel.TotalPages.Should().Be(0);
        _testOrganisationPponSearchModel.Id.Should().Be(DefaultOrganisationId);
    }

    [Fact]
    public async Task OnGet_WhenCalledWithSpecialCharactersOnly_ShouldReturnEmptyResults()
    {
        SetupRouteData(DefaultOrganisationId);

        var result = await _testOrganisationPponSearchModel.OnGet(DefaultPageNumber, "@#$%");

        result.Should().BeOfType<PageResult>();
        _testOrganisationPponSearchModel.Organisations.Should().BeEmpty();
        _testOrganisationPponSearchModel.TotalOrganisations.Should().Be(0);
        _testOrganisationPponSearchModel.TotalPages.Should().Be(0);
        _testOrganisationPponSearchModel.Id.Should().Be(DefaultOrganisationId);
    }

    [Fact]
    public async Task OnGet_WhenCalledWithValidSearchText_ShouldReturnResults()
    {
        SetupRouteData(DefaultOrganisationId);
        SetupRefererHeader();

        var mockResults = new List<OrganisationSearchByPponResult>
        {
            CreateTestOrganisationResult("Test Organisation 1", new List<PartyRole> { PartyRole.Buyer }),
            CreateTestOrganisationResult("Test Organisation 2", new List<PartyRole> { PartyRole.Buyer })
        };

        SetupSuccessfulSearch(DefaultSearchText, DefaultSortOrder, DefaultPageSize, 0, mockResults);

        var result = await _testOrganisationPponSearchModel.OnGet(DefaultPageNumber, DefaultSearchText);

        result.Should().BeOfType<PageResult>();
        _testOrganisationPponSearchModel.Organisations.Should().HaveCount(2);
        _testOrganisationPponSearchModel.TotalOrganisations.Should().Be(2);
        _testOrganisationPponSearchModel.TotalPages.Should().Be(1);
        _testOrganisationPponSearchModel.Organisations.ElementAt(0).Name.Should().Be("Test Organisation 1");
        _testOrganisationPponSearchModel.Organisations.ElementAt(1).Name.Should().Be("Test Organisation 2");
        _testOrganisationPponSearchModel.Id.Should().Be(DefaultOrganisationId);
    }

    [Fact]
    public void FormatAddresses_WhenAddressesIsEmpty_ShouldReturnNotApplicable()
    {
        var result = _testOrganisationPponSearchModel.FormatAddresses(new List<OrganisationAddress>());
        result.Should().Be("N/A");
    }

    [Fact]
    public void FormatAddresses_WithCompleteAddress_ShouldFormatCorrectly()
    {
        var addresses = new List<OrganisationAddress>
        {
            new OrganisationAddress(
                country: "United Kingdom",
                streetAddress: "123 Test Street",
                locality: "Test City",
                region: "Test Region",
                postalCode: "AB12 3CD",
                countryName: "",
                type: AddressType.Postal)
        };

        var result = _testOrganisationPponSearchModel.FormatAddresses(addresses);
        result.Should().Be("123 Test Street, Test City, Test Region, AB12 3CD, United Kingdom");
    }

    [Fact]
    public void FormatAddresses_WithPartialAddress_ShouldFormatWithAvailableParts()
    {
        var addresses = new List<OrganisationAddress>
        {
            new(
                country: "",
                streetAddress: "123 Test Street",
                locality: null,
                region: "Test Region",
                postalCode: null,
                countryName: "United Kingdom",
                type: AddressType.Postal)
        };

        var result = _testOrganisationPponSearchModel.FormatAddresses(addresses);
        result.Should().Be("123 Test Street, Test Region");
    }

    [Fact]
    public void FormatAddresses_WithEmptyAddressParts_ShouldReturnNotApplicable()
    {
        var addresses = new List<OrganisationAddress>
        {
            new(
                country: "",
                streetAddress: null,
                locality: null,
                region: null,
                postalCode: null,
                countryName: null,
                type: AddressType.Postal)
        };

        var result = _testOrganisationPponSearchModel.FormatAddresses(addresses);
        result.Should().Be("N/A");
    }

    [Theory]
    [InlineData(PartyRole.Buyer, "govuk-tag--blue")]
    [InlineData(PartyRole.Supplier, "govuk-tag--purple")]
    [InlineData(PartyRole.Tenderer, "govuk-tag--yellow")]
    [InlineData(PartyRole.Funder, "govuk-tag--red")]
    public void GetTagClassForRole_ShouldReturnCorrectTagClass(PartyRole role, string expectedTagClass)
    {
        var result = _testOrganisationPponSearchModel.GetTagClassForRole(role);
        result.Should().Be(expectedTagClass);
    }

    [Theory]
    [InlineData("asc")]
    [InlineData("desc")]
    [InlineData("rel")]
    public async Task OnGet_WithDifferentSortOrders_PassesCorrectSortOrderToAPI(string sortOrder)
    {
        SetupRouteData(DefaultOrganisationId);
        SetupRefererHeader();
        var mockResults = new List<OrganisationSearchByPponResult>
            { CreateTestOrganisationResult("Test Organisation",new List<PartyRole> { PartyRole.Buyer }) };

        _mockOrganisationClient.Setup(m => m.SearchByNameOrPponAsync(
                DefaultSearchText,
                DefaultPageSize,
                0,
                sortOrder))
            .ReturnsAsync(CreateTestResponse(mockResults))
            .Verifiable();

        await _testOrganisationPponSearchModel.OnGet(DefaultPageNumber, DefaultSearchText, sortOrder);

        _mockOrganisationClient.Verify(m => m.SearchByNameOrPponAsync(
                DefaultSearchText,
                DefaultPageSize,
                0,
                sortOrder),
            Times.Once);
        _testOrganisationPponSearchModel.Id.Should().Be(DefaultOrganisationId);
    }

    [Fact]
    public async Task OnGet_WithSpecificPageNumber_UsesCorrectSkipValue()
    {
        const int pageNumber = 3; // Third page
        const int expectedSkip = 20; // (3-1) * 10 = 20
        const int totalCount = 25;

        SetupRouteData(DefaultOrganisationId);
        SetupRefererHeader();
        var mockResults = new List<OrganisationSearchByPponResult>
            { CreateTestOrganisationResult("Test Organisation", new List<PartyRole> { PartyRole.Buyer }) };

        var response = new OrganisationSearchByPponResponse(mockResults, totalCount);

        _mockOrganisationClient.Setup(m => m.SearchByNameOrPponAsync(
                DefaultSearchText,
                DefaultPageSize,
                expectedSkip,
                DefaultSortOrder))
            .ReturnsAsync(response)
            .Verifiable();

        await _testOrganisationPponSearchModel.OnGet(pageNumber, DefaultSearchText);

        _mockOrganisationClient.Verify(m => m.SearchByNameOrPponAsync(
                DefaultSearchText,
                DefaultPageSize,
                expectedSkip,
                DefaultSortOrder),
            Times.Once);

        _testOrganisationPponSearchModel.CurrentPage.Should().Be(pageNumber);
        _testOrganisationPponSearchModel.Skip.Should().Be(expectedSkip);
        _testOrganisationPponSearchModel.TotalPages.Should().Be(3);
        _testOrganisationPponSearchModel.Id.Should().Be(DefaultOrganisationId);
    }

    [Fact]
    public async Task OnGet_WhenApiThrowsException_LogsErrorAndResetsResults()
    {
        int pageNumber = 1;
        string searchText = "Test Organisation";
        string sortOrder = "rel";

        SetupRouteData(DefaultOrganisationId);
        SetupRefererHeader();

        _testOrganisationPponSearchModel.Organisations = new List<OrganisationSearchByPponResult>
        {
            new(
                new List<OrganisationAddress>(),
                Guid.NewGuid(),
                new List<Identifier>(),
                "Initial Test Org",
                new List<PartyRole>(),
                OrganisationType.Organisation
            )
        };
        _testOrganisationPponSearchModel.TotalOrganisations = 1;
        _testOrganisationPponSearchModel.TotalPages = 1;

        var exception = new HttpRequestException("API Error", null, System.Net.HttpStatusCode.InternalServerError);
        _mockOrganisationClient.Setup(m => m.SearchByNameOrPponAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>()))
            .ThrowsAsync(exception);

        await _testOrganisationPponSearchModel.OnGet(pageNumber, searchText, sortOrder);

        _testOrganisationPponSearchModel.Organisations.Should().BeEmpty();
        _testOrganisationPponSearchModel.TotalOrganisations.Should().Be(0);
        _testOrganisationPponSearchModel.TotalPages.Should().Be(0);
        _testOrganisationPponSearchModel.Id.Should().Be(DefaultOrganisationId);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Error occurred while searching for organisations")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnGet_WithOrganisationId_SetsIdProperty()
    {
        SetupRefererHeader();
        SetupRouteData(DefaultOrganisationId);

        await _testOrganisationPponSearchModel.OnGet(DefaultPageNumber, DefaultSearchText);

        _testOrganisationPponSearchModel.Id.Should().Be(DefaultOrganisationId);
    }

    [Fact]
    public async Task OnGet_WithOrganisationId_ShouldUseIdInRefererCheck()
    {
        SetupRefererHeader();
        SetupRouteData(DefaultOrganisationId);

        var mockResults = new List<OrganisationSearchByPponResult>
        {
            CreateTestOrganisationResult("Test Organisation", new List<PartyRole> { PartyRole.Buyer })
        };

        SetupSuccessfulSearch(DefaultSearchText, DefaultSortOrder, DefaultPageSize, 0, mockResults);

        var result = await _testOrganisationPponSearchModel.OnGet(DefaultPageNumber, DefaultSearchText, DefaultSortOrder);

        result.Should().BeOfType<PageResult>();
        _testOrganisationPponSearchModel.Organisations.Should().HaveCount(1);
        _testOrganisationPponSearchModel.Id.Should().Be(DefaultOrganisationId);
    }

    [Fact]
    public async Task OnGet_WithOrganisationId_PassesCorrectReferrerPathToInitModel()
    {
        SetupRefererHeader();
        SetupRouteData(DefaultOrganisationId);

        await _testOrganisationPponSearchModel.OnGet(DefaultPageNumber, DefaultSearchText, DefaultSortOrder);

        _testOrganisationPponSearchModel.Organisations.Should().BeEmpty();
    }

    [Fact]
    public async Task OnGet_WithDifferentOrganisationId_ShouldNotFindReferer()
    {
        var differentId = Guid.NewGuid();
        _testOrganisationPponSearchModel.Request.Headers["Referer"] = $"https://example.com/organisation/{differentId}/buyer/search";
        SetupRouteData(DefaultOrganisationId);

        await _testOrganisationPponSearchModel.OnGet(DefaultPageNumber, DefaultSearchText, DefaultSortOrder);

        _testOrganisationPponSearchModel.Organisations.Should().BeEmpty();
        _testOrganisationPponSearchModel.Id.Should().Be(DefaultOrganisationId);
    }

    private void SetupRouteData(Guid organisationId)
    {
        var routeData = new RouteData();
        routeData.Values["id"] = organisationId.ToString();
        _testOrganisationPponSearchModel.PageContext.RouteData = routeData;
    }
}