using CO.CDP.RegisterOfCommercialTools.App.Services;
using CO.CDP.UI.Foundation.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.App.Pages;
using SearchModel = CO.CDP.RegisterOfCommercialTools.App.Models.SearchModel;

namespace CO.CDP.RegisterOfCommercialTools.App.Tests.Pages.Search;

public class IndexModelTest
{
    private readonly Mock<ISearchService> _mockSearchService;
    private readonly IndexModel _model;

    public IndexModelTest()
    {
        _mockSearchService = new Mock<ISearchService>();
        var mockSirsiUrlService = new Mock<ISirsiUrlService>();
        mockSirsiUrlService.Setup(s => s.BuildUrl("/", null, null)).Returns("https://sirsi.home/");
        _model = new IndexModel(_mockSearchService.Object, mockSirsiUrlService.Object);
    }

    [Fact]
    public async Task OnGet_ShouldPopulateSearchResults()
    {
        var searchResults = new List<SearchResult>
        {
            new(Guid.NewGuid(), "Test Result", "Test Caption", "Test Tool", SearchResultStatus.Active, "1%", "Yes",
                "2025-01-01",
                "2025-01-01 to 2025-12-31", "Direct Award")
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
        _model.SearchParams.FeeFrom.Should().BeNull();
        _model.SearchParams.FeeTo.Should().BeNull();
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

        var expectedOpenAccordions = new[]
        {
            "commercial-tool", "commercial-tool-status", "contracting-authority-usage", "award-method", "fees",
            "date-range"
        };
        _model.OpenAccordions.Should().BeEquivalentTo(expectedOpenAccordions);
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
        _model.SearchParams.FeeFrom = 0;
        _model.SearchParams.FeeTo = 100;
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
        _model.SearchParams.FeeFrom.Should().Be(0);
        _model.SearchParams.FeeTo.Should().Be(100);
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
    public void OnPostReset_ShouldClearModelStateAndRedirect()
    {
        _model.ModelState.AddModelError("test", "test error");

        var result = _model.OnPostReset();

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().BeNull();
        redirectResult.RouteValues.Should().BeNull();
    }


    [Fact]
    public async Task OnGetAsync_SetsSirsiHomeUrl()
    {
        await _model.OnGetAsync();
        _model.SirsiHomeUrl.Should().Be("https://sirsi.home/");
    }

    [Fact]
    public async Task OnGet_ShouldSetTotalCount()
    {
        var searchResults = new List<SearchResult>
        {
            new(Guid.NewGuid(), "Test Result", "Test Caption", "Test Tool", SearchResultStatus.Active, "1%", "Yes",
                "2025-01-01",
                "2025-01-01 to 2025-12-31", "Direct Award")
        };
        _mockSearchService.Setup(s => s.SearchAsync(It.IsAny<SearchModel>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((searchResults, 42));

        await _model.OnGetAsync();

        _model.TotalCount.Should().Be(42);
    }
}