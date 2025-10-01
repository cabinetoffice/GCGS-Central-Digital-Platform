using CO.CDP.RegisterOfCommercialTools.App.Services;
using CO.CDP.UI.Foundation.Services;
using FluentAssertions;
using Moq;
using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.App.Pages;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using SearchModel = CO.CDP.RegisterOfCommercialTools.App.Models.SearchModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace CO.CDP.RegisterOfCommercialTools.App.Tests.Pages.Search;

public class IndexModelTest
{
    private readonly Mock<ISearchService> _mockSearchService;
    private readonly IndexModel _model;

    public IndexModelTest()
    {
        _mockSearchService = new Mock<ISearchService>();
        _mockSearchService.Setup(s => s.SearchAsync(It.IsAny<SearchModel>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((new List<SearchResult>(), 0));

        var mockSirsiUrlService = new Mock<ISirsiUrlService>();
        mockSirsiUrlService.Setup(s => s.BuildUrl("/", null, null, null)).Returns("https://sirsi.home/");
        mockSirsiUrlService.Setup(s => s.BuildUrl(It.Is<string>(path => path.Contains("/organisation/") && path.Contains("/buyer")), null, null, null))
            .Returns<string, Guid?, string?, bool?>((path, _, _, _) => $"https://sirsi.home{path}");
        var mockFtsUrlService = new Mock<IFtsUrlService>();
        mockFtsUrlService.Setup(s => s.BuildUrl(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<bool?>())).Returns("https://fts.test/");
        var mockLogger = new Mock<ILogger<IndexModel>>();
        _model = new IndexModel(_mockSearchService.Object, mockSirsiUrlService.Object, mockFtsUrlService.Object, mockLogger.Object);

        var mockHttpContext = new Mock<HttpContext>();
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Path).Returns("/");
        mockRequest.Setup(r => r.QueryString).Returns(QueryString.Empty);
        mockRequest.Setup(r => r.Query).Returns(new QueryCollection());
        mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);

        var pageContext = new PageContext { HttpContext = mockHttpContext.Object };
        _model.PageContext = pageContext;
    }

    [Fact]
    public async Task OnGet_ShouldPopulateSearchResults()
    {
        var searchResults = new List<SearchResult>
        {
            new("003033-2025", "Test Result", "Test Caption", "Test Tool", CommercialToolStatus.Active, "1%", "Yes",
                "2025-01-01",
                "2025-01-01 to 2025-12-31", "Direct Award", "https://fts.test/")
        };
        _mockSearchService.Setup(s => s.SearchAsync(It.IsAny<SearchModel>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((searchResults, 1));

        await _model.OnGetAsync();

        _model.SearchResults.Should().BeEquivalentTo(searchResults);
        _model.Pagination.Should().NotBeNull();
        _model.Pagination?.TotalItems.Should().Be(1);
    }

    [Fact]
    public void SearchParams_ShouldBeInitialized()
    {
        _model.SearchParams.Should().NotBeNull();
        _model.SearchParams.Keywords.Should().BeNull();
        _model.SearchParams.SortOrder.Should().BeNull();
        _model.SearchParams.FrameworkOptions.Should().BeNull();
        _model.SearchParams.DynamicMarketOptions.Should().BeNull();
        _model.SearchParams.AwardMethod.Should().BeNull();
        _model.SearchParams.Status.Should().BeEmpty();
        _model.SearchParams.ContractingAuthorityUsage.Should().BeNull();
        _model.SearchParams.FeeMin.Should().BeNull();
        _model.SearchParams.FeeMax.Should().BeNull();
        _model.SearchParams.NoFees.Should().BeNull();
        _model.SearchParams.SubmissionDeadline.From.Day.Should().BeNull();
        _model.SearchParams.SubmissionDeadline.From.Month.Should().BeNull();
        _model.SearchParams.SubmissionDeadline.From.Year.Should().BeNull();
        _model.SearchParams.SubmissionDeadline.To.Day.Should().BeNull();
        _model.SearchParams.SubmissionDeadline.To.Month.Should().BeNull();
        _model.SearchParams.SubmissionDeadline.To.Year.Should().BeNull();
        _model.SearchParams.ContractStartDate.From.Day.Should().BeNull();
        _model.SearchParams.ContractStartDate.From.Month.Should().BeNull();
        _model.SearchParams.ContractStartDate.From.Year.Should().BeNull();
        _model.SearchParams.ContractStartDate.To.Day.Should().BeNull();
        _model.SearchParams.ContractStartDate.To.Month.Should().BeNull();
        _model.SearchParams.ContractStartDate.To.Year.Should().BeNull();
        _model.SearchParams.ContractEndDate.From.Day.Should().BeNull();
        _model.SearchParams.ContractEndDate.From.Month.Should().BeNull();
        _model.SearchParams.ContractEndDate.From.Year.Should().BeNull();
        _model.SearchParams.ContractEndDate.To.Day.Should().BeNull();
        _model.SearchParams.ContractEndDate.To.Month.Should().BeNull();
        _model.SearchParams.ContractEndDate.To.Year.Should().BeNull();
    }

    [Fact]
    public async Task OnGet_WithSearchParams_ShouldRetainSearchParams()
    {
        _model.SearchParams.Keywords = "test";
        _model.SearchParams.SortOrder = "a-z";
        _model.SearchParams.FrameworkOptions = "open";
        _model.SearchParams.DynamicMarketOptions = "utilities-only";
        _model.SearchParams.AwardMethod = "direct-award";
        _model.SearchParams.Status = ["upcoming", "active-buyers"];
        _model.SearchParams.ContractingAuthorityUsage = "yes";
        _model.SearchParams.FeeMin = 0;
        _model.SearchParams.FeeMax = 100;
        _model.SearchParams.NoFees = "true";
        _model.SearchParams.SubmissionDeadline.From.Day = "1";
        _model.SearchParams.SubmissionDeadline.From.Month = "1";
        _model.SearchParams.SubmissionDeadline.From.Year = "2025";
        _model.SearchParams.SubmissionDeadline.To.Day = "31";
        _model.SearchParams.SubmissionDeadline.To.Month = "1";
        _model.SearchParams.SubmissionDeadline.To.Year = "2025";
        _model.SearchParams.ContractStartDate.From.Day = "1";
        _model.SearchParams.ContractStartDate.From.Month = "2";
        _model.SearchParams.ContractStartDate.From.Year = "2025";
        _model.SearchParams.ContractStartDate.To.Day = "28";
        _model.SearchParams.ContractStartDate.To.Month = "2";
        _model.SearchParams.ContractStartDate.To.Year = "2025";
        _model.SearchParams.ContractEndDate.From.Day = "1";
        _model.SearchParams.ContractEndDate.From.Month = "1";
        _model.SearchParams.ContractEndDate.From.Year = "2026";
        _model.SearchParams.ContractEndDate.To.Day = "31";
        _model.SearchParams.ContractEndDate.To.Month = "1";
        _model.SearchParams.ContractEndDate.To.Year = "2026";

        _mockSearchService.Setup(s => s.SearchAsync(_model.SearchParams, 1, 10))
            .ReturnsAsync((new List<SearchResult>(), 0));

        await _model.OnGetAsync();

        _model.SearchParams.Keywords.Should().Be("test");
        _model.SearchParams.SortOrder.Should().Be("a-z");
        _model.SearchParams.FrameworkOptions.Should().Be("open");
        _model.SearchParams.DynamicMarketOptions.Should().Be("utilities-only");
        _model.SearchParams.AwardMethod.Should().Be("direct-award");
        _model.SearchParams.Status.Should().BeEquivalentTo(["upcoming", "active-buyers"]);
        _model.SearchParams.ContractingAuthorityUsage.Should().Be("yes");
        _model.SearchParams.FeeMin.Should().Be(0);
        _model.SearchParams.FeeMax.Should().Be(100);
        _model.SearchParams.NoFees.Should().Be("true");
        _model.SearchParams.SubmissionDeadline.From.Day.Should().Be("1");
        _model.SearchParams.SubmissionDeadline.From.Month.Should().Be("1");
        _model.SearchParams.SubmissionDeadline.From.Year.Should().Be("2025");
        _model.SearchParams.SubmissionDeadline.To.Day.Should().Be("31");
        _model.SearchParams.SubmissionDeadline.To.Month.Should().Be("1");
        _model.SearchParams.SubmissionDeadline.To.Year.Should().Be("2025");
        _model.SearchParams.ContractStartDate.From.Day.Should().Be("1");
        _model.SearchParams.ContractStartDate.From.Month.Should().Be("2");
        _model.SearchParams.ContractStartDate.From.Year.Should().Be("2025");
        _model.SearchParams.ContractStartDate.To.Day.Should().Be("28");
        _model.SearchParams.ContractStartDate.To.Month.Should().Be("2");
        _model.SearchParams.ContractStartDate.To.Year.Should().Be("2025");
        _model.SearchParams.ContractEndDate.From.Day.Should().Be("1");
        _model.SearchParams.ContractEndDate.From.Month.Should().Be("1");
        _model.SearchParams.ContractEndDate.From.Year.Should().Be("2026");
        _model.SearchParams.ContractEndDate.To.Day.Should().Be("31");
        _model.SearchParams.ContractEndDate.To.Month.Should().Be("1");
        _model.SearchParams.ContractEndDate.To.Year.Should().Be("2026");
    }




    [Fact]
    public async Task OnGetAsync_SetsSirsiHomeUrl()
    {
        await _model.OnGetAsync();
    }

    [Fact]
    public async Task OnGet_ShouldSetTotalCount()
    {
        var searchResults = new List<SearchResult>
        {
            new("003033-2025", "Test Result", "Test Caption", "Test Tool", CommercialToolStatus.Active, "1%", "Yes",
                "2025-01-01",
                "2025-01-01 to 2025-12-31", "Direct Award", null)
        };
        _mockSearchService.Setup(s => s.SearchAsync(It.IsAny<SearchModel>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((searchResults, 42));

        await _model.OnGetAsync();

        _model.TotalCount.Should().Be(42);
    }


    [Fact]
    public async Task OnGetAsync_WithOriginBuyerViewAndOrganisationId_SetsHomeUrlToBuyerView()
    {
        var organisationId = Guid.NewGuid();
        _model.Origin = "buyer-view";
        _model.OrganisationId = organisationId;

        await _model.OnGetAsync();

        _model.HomeUrl.Should().Be($"https://sirsi.home/organisation/{organisationId}/buyer");
    }

    [Fact]
    public async Task OnGetAsync_WithOriginBuyerViewButNoOrganisationId_SetsHomeUrlToFts()
    {
        _model.Origin = "buyer-view";
        _model.OrganisationId = null;

        await _model.OnGetAsync();

        _model.HomeUrl.Should().Be("https://fts.test/");
    }

    [Fact]
    public async Task OnGetAsync_WithNoOrigin_SetsHomeUrlToFts()
    {
        var organisationId = Guid.NewGuid();
        _model.Origin = null;
        _model.OrganisationId = organisationId;

        await _model.OnGetAsync();

        _model.HomeUrl.Should().Be("https://fts.test/");
    }

    [Fact]
    public async Task OnGetAsync_WithDifferentOrigin_SetsHomeUrlToFts()
    {
        var organisationId = Guid.NewGuid();
        _model.Origin = "other-origin";
        _model.OrganisationId = organisationId;

        await _model.OnGetAsync();

        _model.HomeUrl.Should().Be("https://fts.test/");
    }

    [Fact]
    public async Task OnGetAsync_WithNoOriginAndNoOrganisationId_SetsHomeUrlToFts()
    {
        _model.Origin = null;
        _model.OrganisationId = null;

        await _model.OnGetAsync();

        _model.HomeUrl.Should().Be("https://fts.test/");
    }
}