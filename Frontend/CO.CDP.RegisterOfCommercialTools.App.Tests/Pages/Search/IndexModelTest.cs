using CO.CDP.RegisterOfCommercialTools.App.Pages.Search;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.RegisterOfCommercialTools.App.Tests.Pages.Search;

public class IndexModelTest
{
    private readonly IndexModel _model = new();

    [Fact]
    public void OnGet_ShouldPopulateSearchResults()
    {
        _model.OnGet();

        _model.SearchResults.Should().NotBeEmpty();
        _model.Pagination.Should().NotBeNull();
    }

    [Fact]
    public void SearchParams_ShouldBeInitialized()
    {
        _model.SearchParams.Should().NotBeNull();
        _model.SearchParams.Keywords.Should().BeNull();
        _model.SearchParams.SortOrder.Should().BeNull();
        _model.SearchParams.FrameworkOptions.Should().BeNull();
        _model.SearchParams.DynamicMarketOptions.Should().BeNull();
        _model.SearchParams.AwardMethod.Should().BeFalse();
        _model.SearchParams.AwardMethodSet.Should().BeFalse();
        _model.SearchParams.Status.Should().BeEmpty();
        _model.SearchParams.ContractingAuthorityUsage.Should().BeFalse();
        _model.SearchParams.ContractingAuthorityUsageSet.Should().BeFalse();
        _model.SearchParams.FeeFrom.Should().BeNull();
        _model.SearchParams.FeeTo.Should().BeNull();
        _model.SearchParams.NoFees.Should().BeFalse();
        _model.SearchParams.NoFeesSet.Should().BeFalse();
        _model.SearchParams.SubmissionDeadlineFrom.Should().BeNull();
        _model.SearchParams.SubmissionDeadlineTo.Should().BeNull();
        _model.SearchParams.ContractStartDateFrom.Should().BeNull();
        _model.SearchParams.ContractStartDateTo.Should().BeNull();
        _model.SearchParams.ContractEndDateFrom.Should().BeNull();
        _model.SearchParams.ContractEndDateTo.Should().BeNull();
    }

    [Fact]
    public void OnGet_WithSearchParams_ShouldRetainSearchParams()
    {
        var searchParams = new SearchModel
        {
            Keywords = "test",
            SortOrder = "a-z",
            FrameworkOptions = "open",
            DynamicMarketOptions = "utilities-only",
            AwardMethod = true,
            Status = ["upcoming", "active-buyers"],
            ContractingAuthorityUsage = true,
            FeeFrom = 0,
            FeeTo = 100,
            NoFees = true,
            SubmissionDeadlineFrom = new DateOnly(2025, 1, 1),
            SubmissionDeadlineTo = new DateOnly(2025, 1, 31),
            ContractStartDateFrom = new DateOnly(2025, 2, 1),
            ContractStartDateTo = new DateOnly(2025, 2, 28),
            ContractEndDateFrom = new DateOnly(2026, 1, 1),
            ContractEndDateTo = new DateOnly(2026, 1, 31)
        };
        _model.SearchParams = searchParams;

        _model.OnGet();

        _model.SearchParams.Should().Be(searchParams);
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
}