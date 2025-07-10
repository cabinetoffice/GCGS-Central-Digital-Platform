using CO.CDP.RegisterOfCommercialTools.App.Pages.Search;
using FluentAssertions;

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
        _model.SearchParams.Status.Should().BeEmpty();
        _model.SearchParams.ContractingAuthorityUsage.Should().BeNull();
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
            Status = ["upcoming", "active-buyers"],
            ContractingAuthorityUsage = true
        };
        _model.SearchParams = searchParams;

        _model.OnGet();

        _model.SearchParams.Should().Be(searchParams);
    }
}