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
        mockSirsiUrlService.Setup(s => s.BuildAuthenticatedUrl(It.Is<string>(path => path.Contains("/organisation/") && path.Contains("/buyer")), It.IsAny<Guid?>(), null))
            .Returns<string, Guid?, bool?>((path, orgId, _) => $"https://sirsi.home/one-login/sign-in?redirectUri={Uri.EscapeDataString($"https://sirsi.home{path}?language=en_GB&organisation_id={orgId}")}&language=en_GB");
        var mockFtsUrlService = new Mock<IFtsUrlService>();
        mockFtsUrlService.Setup(s => s.BuildUrl(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<bool?>())).Returns("https://fts.test/");
        var mockCpvCodeService = new Mock<ICpvCodeService>();
        mockCpvCodeService.Setup(s => s.GetByCodesAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<CpvCodeDto>());

        var mockLocationCodeService = new Mock<ILocationCodeService>();
        mockLocationCodeService.Setup(s => s.GetByCodesAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<NutsCodeDto>());
        var mockLogger = new Mock<ILogger<IndexModel>>();
        _model = new IndexModel(_mockSearchService.Object, mockSirsiUrlService.Object, mockFtsUrlService.Object, mockCpvCodeService.Object, mockLocationCodeService.Object, mockLogger.Object);

        var mockHttpContext = new Mock<HttpContext>();
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Path).Returns("/");
        mockRequest.Setup(r => r.QueryString).Returns(QueryString.Empty);
        mockRequest.Setup(r => r.Query).Returns(new QueryCollection());
        mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);

        var actionContext = new Microsoft.AspNetCore.Mvc.ActionContext(mockHttpContext.Object, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
        var pageContext = new PageContext(actionContext)
        {
            ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(), new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary())
        };
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
        _model.SearchParams.FilterFrameworks.Should().BeFalse();
        _model.SearchParams.IsOpenFrameworks.Should().BeFalse();
        _model.SearchParams.FilterDynamicMarkets.Should().BeFalse();
        _model.SearchParams.IsUtilitiesOnly.Should().BeFalse();
        _model.SearchParams.AwardMethod.Should().BeEmpty();
        _model.SearchParams.Status.Should().BeEmpty();
        _model.SearchParams.ContractingAuthorityUsage.Should().BeNull();
        _model.SearchParams.FeeMin.Should().BeNull();
        _model.SearchParams.FeeMax.Should().BeNull();
        _model.SearchParams.NoFees.Should().BeNull();
        _model.SearchParams.SubmissionDeadlineFromDay.Should().BeNull();
        _model.SearchParams.SubmissionDeadlineFromMonth.Should().BeNull();
        _model.SearchParams.SubmissionDeadlineFromYear.Should().BeNull();
        _model.SearchParams.SubmissionDeadlineToDay.Should().BeNull();
        _model.SearchParams.SubmissionDeadlineToMonth.Should().BeNull();
        _model.SearchParams.SubmissionDeadlineToYear.Should().BeNull();
        _model.SearchParams.ContractStartDateFromDay.Should().BeNull();
        _model.SearchParams.ContractStartDateFromMonth.Should().BeNull();
        _model.SearchParams.ContractStartDateFromYear.Should().BeNull();
        _model.SearchParams.ContractStartDateToDay.Should().BeNull();
        _model.SearchParams.ContractStartDateToMonth.Should().BeNull();
        _model.SearchParams.ContractStartDateToYear.Should().BeNull();
        _model.SearchParams.ContractEndDateFromDay.Should().BeNull();
        _model.SearchParams.ContractEndDateFromMonth.Should().BeNull();
        _model.SearchParams.ContractEndDateFromYear.Should().BeNull();
        _model.SearchParams.ContractEndDateToDay.Should().BeNull();
        _model.SearchParams.ContractEndDateToMonth.Should().BeNull();
        _model.SearchParams.ContractEndDateToYear.Should().BeNull();
    }

    [Fact]
    public async Task OnGet_WithSearchParams_ShouldRetainSearchParams()
    {
        _model.SearchParams.Keywords = "test";
        _model.SearchParams.SortOrder = "a-z";
        _model.SearchParams.FilterFrameworks = true;
        _model.SearchParams.IsOpenFrameworks = true;
        _model.SearchParams.FilterDynamicMarkets = true;
        _model.SearchParams.IsUtilitiesOnly = true;
        _model.SearchParams.AwardMethod = ["direct-award"];
        _model.SearchParams.Status = ["upcoming", "active-buyers"];
        _model.SearchParams.ContractingAuthorityUsage = "yes";
        _model.SearchParams.FeeMin = 0;
        _model.SearchParams.FeeMax = 100;
        _model.SearchParams.NoFees = "true";
        _model.SearchParams.SubmissionDeadlineFromDay = "1";
        _model.SearchParams.SubmissionDeadlineFromMonth = "1";
        _model.SearchParams.SubmissionDeadlineFromYear = "2025";
        _model.SearchParams.SubmissionDeadlineToDay = "31";
        _model.SearchParams.SubmissionDeadlineToMonth = "1";
        _model.SearchParams.SubmissionDeadlineToYear = "2025";
        _model.SearchParams.ContractStartDateFromDay = "1";
        _model.SearchParams.ContractStartDateFromMonth = "2";
        _model.SearchParams.ContractStartDateFromYear = "2025";
        _model.SearchParams.ContractStartDateToDay = "28";
        _model.SearchParams.ContractStartDateToMonth = "2";
        _model.SearchParams.ContractStartDateToYear = "2025";
        _model.SearchParams.ContractEndDateFromDay = "1";
        _model.SearchParams.ContractEndDateFromMonth = "1";
        _model.SearchParams.ContractEndDateFromYear = "2026";
        _model.SearchParams.ContractEndDateToDay = "31";
        _model.SearchParams.ContractEndDateToMonth = "1";
        _model.SearchParams.ContractEndDateToYear = "2026";

        _mockSearchService.Setup(s => s.SearchAsync(_model.SearchParams, 1, 10))
            .ReturnsAsync((new List<SearchResult>(), 0));

        await _model.OnGetAsync();

        _model.SearchParams.Keywords.Should().Be("test");
        _model.SearchParams.SortOrder.Should().Be("a-z");
        _model.SearchParams.FilterFrameworks.Should().BeTrue();
        _model.SearchParams.IsOpenFrameworks.Should().BeTrue();
        _model.SearchParams.FilterDynamicMarkets.Should().BeTrue();
        _model.SearchParams.IsUtilitiesOnly.Should().BeTrue();
        _model.SearchParams.AwardMethod.Should().BeEquivalentTo(["direct-award"]);
        _model.SearchParams.Status.Should().BeEquivalentTo(["upcoming", "active-buyers"]);
        _model.SearchParams.ContractingAuthorityUsage.Should().Be("yes");
        _model.SearchParams.FeeMin.Should().Be(0);
        _model.SearchParams.FeeMax.Should().Be(100);
        _model.SearchParams.NoFees.Should().Be("true");
        _model.SearchParams.SubmissionDeadlineFromDay.Should().Be("1");
        _model.SearchParams.SubmissionDeadlineFromMonth.Should().Be("1");
        _model.SearchParams.SubmissionDeadlineFromYear.Should().Be("2025");
        _model.SearchParams.SubmissionDeadlineToDay.Should().Be("31");
        _model.SearchParams.SubmissionDeadlineToMonth.Should().Be("1");
        _model.SearchParams.SubmissionDeadlineToYear.Should().Be("2025");
        _model.SearchParams.ContractStartDateFromDay.Should().Be("1");
        _model.SearchParams.ContractStartDateFromMonth.Should().Be("2");
        _model.SearchParams.ContractStartDateFromYear.Should().Be("2025");
        _model.SearchParams.ContractStartDateToDay.Should().Be("28");
        _model.SearchParams.ContractStartDateToMonth.Should().Be("2");
        _model.SearchParams.ContractStartDateToYear.Should().Be("2025");
        _model.SearchParams.ContractEndDateFromDay.Should().Be("1");
        _model.SearchParams.ContractEndDateFromMonth.Should().Be("1");
        _model.SearchParams.ContractEndDateFromYear.Should().Be("2026");
        _model.SearchParams.ContractEndDateToDay.Should().Be("31");
        _model.SearchParams.ContractEndDateToMonth.Should().Be("1");
        _model.SearchParams.ContractEndDateToYear.Should().Be("2026");
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
    public async Task OnGet_ShouldSetDefaultOpenAccordionsWhenNoAccParam()
    {
        var mockHttpContext = new Mock<HttpContext>();
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Path).Returns("/");
        mockRequest.Setup(r => r.QueryString).Returns(QueryString.Empty);
        mockRequest.Setup(r => r.Query).Returns(new QueryCollection()); // Ensure no 'acc' parameter
        mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);

        var actionContext = new Microsoft.AspNetCore.Mvc.ActionContext(mockHttpContext.Object, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
        var pageContext = new PageContext(actionContext)
        {
            ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(), new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary())
        };
        _model.PageContext = pageContext;

        await _model.OnGetAsync();

        var expectedOpenAccordions = new[]
        {
            "commercial-tool", "commercial-tool-status", "contracting-authority-usage", "award-method",
            "industry-cpv-code", "contract-location", "fees", "date-range"
        };
        _model.OpenAccordions.Should().BeEquivalentTo(expectedOpenAccordions);
    }

    [Fact]
    public async Task OnGetAsync_WithOriginBuyerViewAndOrganisationId_SetsHomeUrlToBuyerView()
    {
        var organisationId = Guid.NewGuid();
        _model.Origin = "buyer-view";
        _model.OrganisationId = organisationId;

        await _model.OnGetAsync();

        _model.HomeUrl.Should().StartWith("https://sirsi.home/one-login/sign-in?");
        _model.HomeUrl.Should().Contain($"%2Forganisation%2F{organisationId}%2Fbuyer");
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