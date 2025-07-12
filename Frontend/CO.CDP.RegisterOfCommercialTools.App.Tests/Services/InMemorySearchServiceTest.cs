using CO.CDP.RegisterOfCommercialTools.App.Pages.Search;
using CO.CDP.RegisterOfCommercialTools.App.Services;
using FluentAssertions;

namespace CO.CDP.RegisterOfCommercialTools.App.Tests.Services;

public class InMemorySearchServiceTest
{
    private readonly ISearchService _searchService = new InMemorySearchService();

    [Theory]
    [InlineData("Framework", 3)]
    [InlineData("delivery", 1)]
    [InlineData("agile delivery", 1)]
    [InlineData("nonexistent", 0)]
    public async Task SearchAsync_WithAnyWord_ReturnsCorrectResults(string keywords, int expectedCount)
    {
        var searchModel = new SearchModel { Keywords = keywords };
        var (results, totalCount) = await _searchService.SearchAsync(searchModel, 1, 10);
        totalCount.Should().Be(expectedCount);
    }

    [Theory]
    [InlineData("Framework + Agile", 1)]
    [InlineData("Framework + nonexistent", 0)]
    public async Task SearchAsync_WithAllWords_ReturnsCorrectResults(string keywords, int expectedCount)
    {
        var searchModel = new SearchModel { Keywords = keywords };
        var (results, totalCount) = await _searchService.SearchAsync(searchModel, 1, 10);
        totalCount.Should().Be(expectedCount);
    }

    [Theory]
    [InlineData("\"Framework for Agile Delivery Services\"", 1)]
    [InlineData("\"Framework for Agile\"", 1)]
    [InlineData("\"nonexistent phrase\"", 0)]
    public async Task SearchAsync_WithExactPhrase_ReturnsCorrectResults(string keywords, int expectedCount)
    {
        var searchModel = new SearchModel { Keywords = keywords };
        var (results, totalCount) = await _searchService.SearchAsync(searchModel, 1, 10);
        totalCount.Should().Be(expectedCount);
    }

    [Fact]
    public async Task SearchAsync_WithEmptyKeywords_ReturnsAllResults()
    {
        var searchModel = new SearchModel { Keywords = "" };
        var (results, totalCount) = await _searchService.SearchAsync(searchModel, 1, 10);
        totalCount.Should().Be(5);
    }
}
