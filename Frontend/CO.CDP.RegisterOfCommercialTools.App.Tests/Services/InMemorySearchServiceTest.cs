using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.App.Pages;
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
        var (_, totalCount) = await _searchService.SearchAsync(searchModel, 1, 10);
        totalCount.Should().Be(expectedCount);
    }

    [Theory]
    [InlineData("Framework + Agile", 1)]
    [InlineData("Framework + nonexistent", 0)]
    public async Task SearchAsync_WithAllWords_ReturnsCorrectResults(string keywords, int expectedCount)
    {
        var searchModel = new SearchModel { Keywords = keywords };
        var (_, totalCount) = await _searchService.SearchAsync(searchModel, 1, 10);
        totalCount.Should().Be(expectedCount);
    }

    [Theory]
    [InlineData("\"Framework for Agile Delivery Services\"", 1)]
    [InlineData("\"Framework for Agile\"", 1)]
    [InlineData("\"nonexistent phrase\"", 0)]
    public async Task SearchAsync_WithExactPhrase_ReturnsCorrectResults(string keywords, int expectedCount)
    {
        var searchModel = new SearchModel { Keywords = keywords };
        var (_, totalCount) = await _searchService.SearchAsync(searchModel, 1, 10);
        totalCount.Should().Be(expectedCount);
    }

    [Fact]
    public async Task SearchAsync_WithEmptyKeywords_ReturnsAllResults()
    {
        var searchModel = new SearchModel { Keywords = "" };
        var (_, totalCount) = await _searchService.SearchAsync(searchModel, 1, 10);
        totalCount.Should().Be(5);
    }

    [Theory]
    [InlineData(null,
        new[]
        {
            "Framework for Agile Delivery Services", "Digital Outcomes and Specialists 6", "G-Cloud 13",
            "Vehicle Telematics/Hardware and Software Solutions", "Gigabit Capable Connectivity"
        })]
    [InlineData("relevance",
        new[]
        {
            "Framework for Agile Delivery Services", "Digital Outcomes and Specialists 6", "G-Cloud 13",
            "Vehicle Telematics/Hardware and Software Solutions", "Gigabit Capable Connectivity"
        })]
    [InlineData("a-z",
        new[]
        {
            "Digital Outcomes and Specialists 6", "Framework for Agile Delivery Services", "G-Cloud 13",
            "Gigabit Capable Connectivity", "Vehicle Telematics/Hardware and Software Solutions"
        })]
    [InlineData("z-a",
        new[]
        {
            "Vehicle Telematics/Hardware and Software Solutions", "Gigabit Capable Connectivity", "G-Cloud 13",
            "Framework for Agile Delivery Services", "Digital Outcomes and Specialists 6"
        })]
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
    [InlineData("z-a", 1, 2,
        new[] { "Vehicle Telematics/Hardware and Software Solutions", "Gigabit Capable Connectivity" })]
    [InlineData("z-a", 2, 2, new[] { "G-Cloud 13", "Framework for Agile Delivery Services" })]
    [InlineData("z-a", 3, 2, new[] { "Digital Outcomes and Specialists 6" })]
    public async Task SearchAsync_PaginatesAndSortsResults(string sortOrder, int pageNumber, int pageSize,
        string[] expectedTitles)
    {
        var searchModel = new SearchModel { SortOrder = sortOrder };
        var (results, totalCount) = await _searchService.SearchAsync(searchModel, pageNumber, pageSize);
        results.Select(r => r.Title).Should().ContainInOrder(expectedTitles);
        totalCount.Should().Be(5);
    }

    [Theory]
    [InlineData("\"Framework for Agile Delivery Services\"", "Framework for Agile Delivery Services", 1000)]
    [InlineData("agile delivery", "Framework for Agile Delivery Services", 500 + 100 + 20)]
    [InlineData("agile", "Framework for Agile Delivery Services", 100 + 20)]
    [InlineData("cloud", "Cloud Hosting and Support", 100 + 10)]
    [InlineData("market research", "Market Research Services", 500 + 100 + 20)]
    [InlineData("nonexistent", "Framework for Agile Delivery Services", 0)]
    public void CalculateRelevanceScore_ReturnsExpectedScore(string keywords, string title, int expectedMinScore)
    {
        var result = new SearchResult(
            Guid.NewGuid(),
            title,
            "Test Caption",
            title,
            SearchResultStatus.Active,
            "1%",
            "Yes",
            "2025-01-01",
            "2025-01-01 to 2025-12-31",
            "Direct Award");
        var score = typeof(InMemorySearchService)
            .GetMethod("CalculateRelevanceScore",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, [result, keywords]);
        ((int)score!).Should().BeGreaterThanOrEqualTo(expectedMinScore);
    }

    [Fact]
    public async Task SearchAsync_RelevanceSort_ReturnsResultsInExpectedOrder()
    {
        var searchModel = new SearchModel { Keywords = "agile delivery", SortOrder = "relevance" };
        var (results, _) = await _searchService.SearchAsync(searchModel, 1, 10);
        results.First().Title.ToLowerInvariant().Should().Contain("agile");
    }

    [Theory]
    [InlineData("11111111-1111-1111-1111-111111111111", "Framework for Agile Delivery Services")]
    [InlineData("22222222-2222-2222-2222-222222222222", "Digital Outcomes and Specialists 6")]
    [InlineData("33333333-3333-3333-3333-333333333333", "G-Cloud 13")]
    [InlineData("44444444-4444-4444-4444-444444444444", "Vehicle Telematics/Hardware and Software Solutions")]
    [InlineData("55555555-5555-5555-5555-555555555555", "Gigabit Capable Connectivity")]
    public async Task GetByIdAsync_ReturnsExpectedResult_WhenIdExists(string guid, string expectedTitle)
    {
        var id = Guid.Parse(guid);

        var result = await _searchService.GetByIdAsync(id);

        result.Should().NotBeNull();
        result.Title.Should().Be(expectedTitle);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenIdDoesNotExist()
    {
        var id = Guid.NewGuid();

        var result = await _searchService.GetByIdAsync(id);

        result.Should().BeNull();
    }
}