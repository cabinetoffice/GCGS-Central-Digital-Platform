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

    [Theory]
    [InlineData(null, new[] { "Framework for Agile Delivery Services", "Digital Outcomes and Specialists 6", "G-Cloud 13", "Vehicle Telematics/Hardware and Software Solutions", "Gigabit Capable Connectivity" })]
    [InlineData("relevance", new[] { "Framework for Agile Delivery Services", "Digital Outcomes and Specialists 6", "G-Cloud 13", "Vehicle Telematics/Hardware and Software Solutions", "Gigabit Capable Connectivity" })]
    [InlineData("a-z", new[] { "Digital Outcomes and Specialists 6", "Framework for Agile Delivery Services", "G-Cloud 13", "Gigabit Capable Connectivity", "Vehicle Telematics/Hardware and Software Solutions" })]
    [InlineData("z-a", new[] { "Vehicle Telematics/Hardware and Software Solutions", "Gigabit Capable Connectivity", "G-Cloud 13", "Framework for Agile Delivery Services", "Digital Outcomes and Specialists 6" })]
    public async Task SearchAsync_SortsResultsBySortOrder(string? sortOrder, string[] expectedTitles)
    {
        var searchModel = new SearchModel { SortOrder = sortOrder };
        var (results, totalCount) = await _searchService.SearchAsync(searchModel, 1, 10);
        results.Select(r => r.Title).Should().ContainInOrder(expectedTitles);
        totalCount.Should().Be(5);
    }

    [Theory]
    [InlineData("a-z", 1, 2, new[] { "Digital Outcomes and Specialists 6", "Framework for Agile Delivery Services" })]
    [InlineData("a-z", 2, 2, new[] { "G-Cloud 13", "Gigabit Capable Connectivity" })]
    [InlineData("a-z", 3, 2, new[] { "Vehicle Telematics/Hardware and Software Solutions" })]
    [InlineData("z-a", 1, 2, new[] { "Vehicle Telematics/Hardware and Software Solutions", "Gigabit Capable Connectivity" })]
    [InlineData("z-a", 2, 2, new[] { "G-Cloud 13", "Framework for Agile Delivery Services" })]
    [InlineData("z-a", 3, 2, new[] { "Digital Outcomes and Specialists 6" })]
    public async Task SearchAsync_PaginatesAndSortsResults(string sortOrder, int pageNumber, int pageSize, string[] expectedTitles)
    {
        var searchModel = new SearchModel { SortOrder = sortOrder };
        var (results, totalCount) = await _searchService.SearchAsync(searchModel, pageNumber, pageSize);
        results.Select(r => r.Title).Should().ContainInOrder(expectedTitles);
        totalCount.Should().Be(5);
    }
}
