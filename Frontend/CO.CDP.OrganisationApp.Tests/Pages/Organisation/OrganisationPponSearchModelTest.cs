using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Organisation;
using CO.CDP.OrganisationApp.Tests.TestData;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
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
    private static readonly Guid Id = Guid.NewGuid();

    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;
    private readonly OrganisationPponSearchModel _testOrganisationPponSearchModel;
    private readonly Mock<ILogger<OrganisationPponSearchModel>> _mockLogger;

    public OrganisationPponSearchModelTest()
    {
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _mockAuthorizationService = new Mock<IAuthorizationService>();
        _mockLogger = new Mock<ILogger<OrganisationPponSearchModel>>();

        _mockAuthorizationService.Setup(a => a.AuthorizeAsync(
            It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
            It.IsAny<object>(),
            It.IsAny<IAuthorizationRequirement[]>()))
            .ReturnsAsync(AuthorizationResult.Success());

        _testOrganisationPponSearchModel =
            new OrganisationPponSearchModel(_mockOrganisationClient.Object, Mock.Of<ISession>(), _mockLogger.Object)
            {
                Id = Id,
                Pagination = new CO.CDP.OrganisationApp.Pages.Shared.PaginationPartialModel
                {
                    CurrentPage = 1,
                    TotalItems = 0,
                    PageSize = 10,
                    Url = $"/organisation/{Id}/buyer/search?SearchText=&sortOrder={DefaultSortOrder}&pageSize={DefaultPageSize}"
                }
            };

        var httpContext = new DefaultHttpContext();
        _testOrganisationPponSearchModel.PageContext = new PageContext { HttpContext = httpContext };
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
        SetupRouteData(Id);
        _testOrganisationPponSearchModel.PageNumber = DefaultPageNumber;
        _testOrganisationPponSearchModel.SearchText = string.Empty;
        _testOrganisationPponSearchModel.SortOrder = DefaultSortOrder;

        var result = await _testOrganisationPponSearchModel.OnGet();

        result.Should().BeOfType<PageResult>();
        _testOrganisationPponSearchModel.Organisations.Should().BeEmpty();
        _testOrganisationPponSearchModel.TotalOrganisations.Should().Be(0);
        _testOrganisationPponSearchModel.TotalPages.Should().Be(0);
        _testOrganisationPponSearchModel.Id.Should().Be(Id);
    }

    [Fact]
    public async Task OnGet_WhenCalledWithWhitespaceSearchText_ShouldReturnEmptyResults()
    {
        SetupRouteData(Id);
        _testOrganisationPponSearchModel.PageNumber = DefaultPageNumber;
        _testOrganisationPponSearchModel.SearchText = "  ";
        _testOrganisationPponSearchModel.SortOrder = DefaultSortOrder;

        var result = await _testOrganisationPponSearchModel.OnGet();

        result.Should().BeOfType<PageResult>();
        _testOrganisationPponSearchModel.Organisations.Should().BeEmpty();
        _testOrganisationPponSearchModel.TotalOrganisations.Should().Be(0);
        _testOrganisationPponSearchModel.TotalPages.Should().Be(0);
        _testOrganisationPponSearchModel.Id.Should().Be(Id);
    }

    [Fact]
    public async Task OnGet_WhenCalledWithSpecialCharactersOnly_ShouldReturnEmptyResults()
    {
        SetupRouteData(Id);
        _testOrganisationPponSearchModel.PageNumber = DefaultPageNumber;
        _testOrganisationPponSearchModel.SearchText = "@#$%";
        _testOrganisationPponSearchModel.SortOrder = DefaultSortOrder;

        var result = await _testOrganisationPponSearchModel.OnGet();

        result.Should().BeOfType<PageResult>();
        _testOrganisationPponSearchModel.Organisations.Should().BeEmpty();
        _testOrganisationPponSearchModel.TotalOrganisations.Should().Be(0);
        _testOrganisationPponSearchModel.TotalPages.Should().Be(0);
        _testOrganisationPponSearchModel.Id.Should().Be(Id);
    }

    [Fact]
    public async Task OnGet_WhenCalledWithValidSearchText_ShouldReturnResults()
    {
        SetupRouteData(Id);
        _testOrganisationPponSearchModel.PageNumber = DefaultPageNumber;
        _testOrganisationPponSearchModel.SearchText = DefaultSearchText;
        _testOrganisationPponSearchModel.SortOrder = DefaultSortOrder;

        var mockResults = new List<OrganisationSearchByPponResult>
        {
            CreateTestOrganisationResult("Test Organisation 1", new List<PartyRole> { PartyRole.Buyer }),
            CreateTestOrganisationResult("Test Organisation 2", new List<PartyRole> { PartyRole.Buyer })
        };

        SetupSuccessfulSearch(DefaultSearchText, DefaultSortOrder, DefaultPageSize, 0, mockResults);

        var result = await _testOrganisationPponSearchModel.OnGet();

        result.Should().BeOfType<PageResult>();
        _testOrganisationPponSearchModel.Organisations.Should().HaveCount(2);
        _testOrganisationPponSearchModel.TotalOrganisations.Should().Be(2);
        _testOrganisationPponSearchModel.TotalPages.Should().Be(1);
        _testOrganisationPponSearchModel.Organisations.ElementAt(0).Name.Should().Be("Test Organisation 1");
        _testOrganisationPponSearchModel.Organisations.ElementAt(1).Name.Should().Be("Test Organisation 2");
        _testOrganisationPponSearchModel.Id.Should().Be(Id);
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
        SetupRouteData(Id);
        _testOrganisationPponSearchModel.PageNumber = DefaultPageNumber;
        _testOrganisationPponSearchModel.SearchText = DefaultSearchText;
        _testOrganisationPponSearchModel.SortOrder = sortOrder;
        var mockResults = new List<OrganisationSearchByPponResult>
            { CreateTestOrganisationResult("Test Organisation", new List<PartyRole> { PartyRole.Buyer }) };

        _mockOrganisationClient.Setup(m => m.SearchByNameOrPponAsync(
                DefaultSearchText,
                DefaultPageSize,
                0,
                sortOrder))
            .ReturnsAsync(CreateTestResponse(mockResults))
            .Verifiable();

        await _testOrganisationPponSearchModel.OnGet();

        _mockOrganisationClient.Verify(m => m.SearchByNameOrPponAsync(
                DefaultSearchText,
                DefaultPageSize,
                0,
                sortOrder),
            Times.Once);
        _testOrganisationPponSearchModel.Id.Should().Be(Id);
    }

    [Fact]
    public async Task OnGet_WithSpecificPageNumber_UsesCorrectSkipValue()
    {
        const int pageNumber = 3; // Third page
        const int expectedSkip = 20; // (3-1) * 10 = 20
        const int totalCount = 25;

        SetupRouteData(Id);
        _testOrganisationPponSearchModel.PageNumber = pageNumber;
        _testOrganisationPponSearchModel.SearchText = DefaultSearchText;
        _testOrganisationPponSearchModel.SortOrder = DefaultSortOrder;
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

        await _testOrganisationPponSearchModel.OnGet();

        _mockOrganisationClient.Verify(m => m.SearchByNameOrPponAsync(
                DefaultSearchText,
                DefaultPageSize,
                expectedSkip,
                DefaultSortOrder),
            Times.Once);

        _testOrganisationPponSearchModel.CurrentPage.Should().Be(pageNumber);
        _testOrganisationPponSearchModel.Skip.Should().Be(expectedSkip);
        _testOrganisationPponSearchModel.TotalPages.Should().Be(3);
        _testOrganisationPponSearchModel.Id.Should().Be(Id);
    }

    [Fact]
    public async Task OnGet_WhenApiThrowsException_LogsError()
    {
        int pageNumber = 1;
        string searchText = "Test Organisation";
        string sortOrder = "rel";

        SetupRouteData(Id);

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
        _testOrganisationPponSearchModel.PageNumber = pageNumber;
        _testOrganisationPponSearchModel.SearchText = searchText;
        _testOrganisationPponSearchModel.SortOrder = sortOrder;

        var exception = new HttpRequestException("API Error", null, System.Net.HttpStatusCode.InternalServerError);
        _mockOrganisationClient.Setup(m => m.SearchByNameOrPponAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>()))
            .ThrowsAsync(exception);

        await _testOrganisationPponSearchModel.OnGet();

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
        SetupRouteData(Id);
        _testOrganisationPponSearchModel.PageNumber = DefaultPageNumber;
        _testOrganisationPponSearchModel.SearchText = DefaultSearchText;
        _testOrganisationPponSearchModel.SortOrder = DefaultSortOrder;
        await _testOrganisationPponSearchModel.OnGet();
        _testOrganisationPponSearchModel.Id.Should().Be(Id);
    }

    [Fact]
    public async Task OnPost_WhenCalledWithValidSearchText_ShouldReturnResults()
    {
        SetupRouteData(Id);
        _testOrganisationPponSearchModel.PageNumber = DefaultPageNumber;
        _testOrganisationPponSearchModel.SearchText = DefaultSearchText;
        _testOrganisationPponSearchModel.SortOrder = DefaultSortOrder;
        var mockResults = new List<OrganisationSearchByPponResult>
        {
            CreateTestOrganisationResult("Test Organisation 1", new List<PartyRole> { PartyRole.Buyer }),
            CreateTestOrganisationResult("Test Organisation 2", new List<PartyRole> { PartyRole.Buyer })
        };
        SetupSuccessfulSearch(DefaultSearchText, DefaultSortOrder, DefaultPageSize, 0, mockResults);
        var result = await _testOrganisationPponSearchModel.OnPost();
        result.Should().BeOfType<PageResult>();
        _testOrganisationPponSearchModel.Organisations.Should().HaveCount(2);
        _testOrganisationPponSearchModel.TotalOrganisations.Should().Be(2);
        _testOrganisationPponSearchModel.TotalPages.Should().Be(1);
        _testOrganisationPponSearchModel.Organisations.ElementAt(0).Name.Should().Be("Test Organisation 1");
        _testOrganisationPponSearchModel.Organisations.ElementAt(1).Name.Should().Be("Test Organisation 2");
        _testOrganisationPponSearchModel.Id.Should().Be(Id);
    }

    [Fact]
    public async Task OnPost_WhenCalledWithInvalidSearchText_ShouldReturnEmptyResults()
    {
        SetupRouteData(Id);
        _testOrganisationPponSearchModel.PageNumber = DefaultPageNumber;
        _testOrganisationPponSearchModel.SearchText = "@#$%";
        _testOrganisationPponSearchModel.SortOrder = DefaultSortOrder;
        var result = await _testOrganisationPponSearchModel.OnPost();
        result.Should().BeOfType<PageResult>();
        _testOrganisationPponSearchModel.Organisations.Should().BeEmpty();
        _testOrganisationPponSearchModel.TotalOrganisations.Should().Be(0);
        _testOrganisationPponSearchModel.TotalPages.Should().Be(0);
        _testOrganisationPponSearchModel.Id.Should().Be(Id);
    }

    [Fact]
    public async Task OnPost_WhenApiThrowsException_LogsErrorAndResetsResults()
    {
        SetupRouteData(Id);
        _testOrganisationPponSearchModel.PageNumber = DefaultPageNumber;
        _testOrganisationPponSearchModel.SearchText = DefaultSearchText;
        _testOrganisationPponSearchModel.SortOrder = DefaultSortOrder;
        var exception = new HttpRequestException("API Error", null, System.Net.HttpStatusCode.InternalServerError);
        _mockOrganisationClient.Setup(m => m.SearchByNameOrPponAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>()))
            .ThrowsAsync(exception);
        await _testOrganisationPponSearchModel.OnPost();
        _testOrganisationPponSearchModel.Organisations.Should().BeEmpty();
        _testOrganisationPponSearchModel.TotalOrganisations.Should().Be(0);
        _testOrganisationPponSearchModel.TotalPages.Should().Be(0);
        _testOrganisationPponSearchModel.Id.Should().Be(Id);
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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task OnPost_EmptySearchText_SetsEnterSearchTermErrorMessage(string? searchText)
    {
        SetupRouteData(Id);
        _testOrganisationPponSearchModel.PageNumber = DefaultPageNumber;
        _testOrganisationPponSearchModel.SearchText = searchText!;
        _testOrganisationPponSearchModel.SortOrder = DefaultSortOrder;
        await _testOrganisationPponSearchModel.OnPost();
        _testOrganisationPponSearchModel.ErrorMessage.Should()
            .Be(Localization.StaticTextResource.Global_EnterSearchTerm);
        _testOrganisationPponSearchModel.Organisations.Should().BeEmpty();
        _testOrganisationPponSearchModel.TotalOrganisations.Should().Be(0);
        _testOrganisationPponSearchModel.TotalPages.Should().Be(0);
    }

    [Theory]
    [InlineData("@#$%")]
    public async Task OnGet_InvalidSearchText_SetsInvalidSearchValueErrorMessage(string searchText)
    {
        SetupRouteData(Id);
        _testOrganisationPponSearchModel.PageNumber = DefaultPageNumber;
        _testOrganisationPponSearchModel.SearchText = searchText;
        _testOrganisationPponSearchModel.SortOrder = DefaultSortOrder;
        await _testOrganisationPponSearchModel.OnGet();
        _testOrganisationPponSearchModel.ErrorMessage.Should()
            .Be(Localization.StaticTextResource.PponSearch_Invalid_Search_Value);
        _testOrganisationPponSearchModel.Organisations.Should().BeEmpty();
        _testOrganisationPponSearchModel.TotalOrganisations.Should().Be(0);
        _testOrganisationPponSearchModel.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task OnGet_NoResults_SetsNoResultsErrorMessage()
    {
        SetupRouteData(Id);
        _testOrganisationPponSearchModel.PageNumber = DefaultPageNumber;
        _testOrganisationPponSearchModel.SearchText = DefaultSearchText;
        _testOrganisationPponSearchModel.SortOrder = DefaultSortOrder;
        SetupSuccessfulSearch(DefaultSearchText, DefaultSortOrder, DefaultPageSize, 0,
            new List<OrganisationSearchByPponResult>());
        await _testOrganisationPponSearchModel.OnGet();
        _testOrganisationPponSearchModel.ErrorMessage.Should().Be(Localization.StaticTextResource.PponSearch_NoResults);
        _testOrganisationPponSearchModel.Organisations.Should().BeEmpty();
        _testOrganisationPponSearchModel.TotalOrganisations.Should().Be(0);
        _testOrganisationPponSearchModel.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task OnGet_ApiThrowsException_SetsNoResultsErrorMessage()
    {
        SetupRouteData(Id);
        _testOrganisationPponSearchModel.PageNumber = DefaultPageNumber;
        _testOrganisationPponSearchModel.SearchText = DefaultSearchText;
        _testOrganisationPponSearchModel.SortOrder = DefaultSortOrder;
        _mockOrganisationClient.Setup(m => m.SearchByNameOrPponAsync(
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>())).ThrowsAsync(new HttpRequestException());
        await _testOrganisationPponSearchModel.OnGet();
        _testOrganisationPponSearchModel.ErrorMessage.Should().Be(Localization.StaticTextResource.PponSearch_NoResults);
        _testOrganisationPponSearchModel.Organisations.Should().BeEmpty();
        _testOrganisationPponSearchModel.TotalOrganisations.Should().Be(0);
        _testOrganisationPponSearchModel.TotalPages.Should().Be(0);
    }

    [Theory]
    [InlineData(PartyRole.Supplier)]
    [InlineData(PartyRole.ProcuringEntity)]
    [InlineData(PartyRole.Tenderer)]
    [InlineData(PartyRole.Funder)]
    [InlineData(PartyRole.Enquirer)]
    [InlineData(PartyRole.Payer)]
    [InlineData(PartyRole.Payee)]
    [InlineData(PartyRole.ReviewBody)]
    [InlineData(PartyRole.InterestedParty)]
    public async Task OnGet_WithNonBuyerRole_RedirectsToPageNotFound(PartyRole role)
    {
        var organisation = OrganisationFactory.CreateOrganisation(id: Id, roles: new List<PartyRole> { role });
        _mockOrganisationClient.Reset();
        _mockOrganisationClient.Setup(c => c.GetOrganisationAsync(Id)).ReturnsAsync(organisation);
        _mockOrganisationClient.Setup(c => c.GetOrganisationLatestMouSignatureAsync(Id)).ThrowsAsync(new ApiException("Not found", 404, "", null, null));
        _mockAuthorizationService.Setup(a => a.AuthorizeAsync(
            It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
            Id,
            It.IsAny<IAuthorizationRequirement[]>()))
            .ReturnsAsync(AuthorizationResult.Failed());
        var model = new OrganisationPponSearchModel(_mockOrganisationClient.Object, Mock.Of<ISession>(), _mockLogger.Object)
        {
            Id = Id,
            Pagination = new CO.CDP.OrganisationApp.Pages.Shared.PaginationPartialModel
            {
                CurrentPage = 1,
                TotalItems = 0,
                PageSize = 10,
                Url = $"/organisation/{Id}/buyer/search?q=&sortOrder=rel"
            }
        };
        SetupRouteData(Id);

        var result = await model.OnGet();

        result.Should().BeOfType<Microsoft.AspNetCore.Mvc.RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    private void SetupRouteData(Guid organisationId)
    {
        var routeData = new RouteData();
        routeData.Values["id"] = organisationId.ToString();
        _testOrganisationPponSearchModel.PageContext.RouteData = routeData;
        _testOrganisationPponSearchModel.PageContext = new PageContext { RouteData = routeData, HttpContext = new DefaultHttpContext() };
    }
}