using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.App.Services;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.RegisterOfCommercialTools.WebApiClient;
using FluentAssertions;
using Moq;

namespace CO.CDP.RegisterOfCommercialTools.App.Tests.Services;

public class SearchServiceTests
{
    private readonly Mock<ICommercialToolsApiClient> _mockApiClient;
    private readonly SearchService _searchService;

    public SearchServiceTests()
    {
        _mockApiClient = new Mock<ICommercialToolsApiClient>();
        _searchService = new SearchService(_mockApiClient.Object);
    }

    [Fact]
    public async Task SearchAsync_ShouldMapSearchModelToRequestDto()
    {
        var searchModel = new SearchModel
        {
            Keywords = "IT services",
            Status = ["Active"],
            FeeMin = 1m,
            FeeMax = 5m,
            AwardMethod = ["With competition"],
            SubmissionDeadlineFromDay = "1",
            SubmissionDeadlineFromMonth = "1",
            SubmissionDeadlineFromYear = "2025",
            SubmissionDeadlineToDay = "31",
            SubmissionDeadlineToMonth = "12",
            SubmissionDeadlineToYear = "2025",
            ContractStartDateFromDay = "1",
            ContractStartDateFromMonth = "6",
            ContractStartDateFromYear = "2025",
            ContractStartDateToDay = "30",
            ContractStartDateToMonth = "6",
            ContractStartDateToYear = "2025"
        };

        var responseDto = new SearchResponse
        {
            Results = new List<SearchResultDto>
            {
                new()
                {
                    Id = "003033-2025",
                    Title = "Test Framework",
                    Description = "Test Description",
                    PublishedDate = DateTime.UtcNow,
                    SubmissionDeadline = DateTime.UtcNow.AddDays(30).ToString("dd MMMM yyyy"),
                    Status = CommercialToolStatus.Active,
                    MaximumFee = "2.5%",
                    AwardMethod = "With competition"
                }
            },
            TotalCount = 25,
            PageNumber = 1,
            PageSize = 10
        };

        _mockApiClient
            .Setup(x => x.SearchAsync(It.IsAny<SearchRequestDto>()))
            .ReturnsAsync(responseDto);

        var (results, totalCount) = await _searchService.SearchAsync(searchModel, 1, 10);

        results.Should().HaveCount(1);
        var result = results.First();
        result.Id.Should().Be("003033-2025");
        result.Title.Should().Be("Test Framework");
        result.Caption.Should().Be("Test Description");
        result.Status.Should().Be(CommercialToolStatus.Active);
        totalCount.Should().Be(25);

        _mockApiClient.Verify(x => x.SearchAsync(It.Is<SearchRequestDto>(dto =>
            dto.Keywords != null && dto.Keywords.Count == 2 && dto.Keywords.Contains("IT") && dto.Keywords.Contains("services") &&
            dto.SearchMode == KeywordSearchMode.Any &&
            dto.MinFees == 0.01m &&
            dto.MaxFees == 0.05m &&
            dto.AwardMethod != null && dto.AwardMethod.Count == 1 && dto.AwardMethod[0] == "With competition" &&
            dto.PageNumber == 1
        )), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WhenNoFeesSelected_ShouldSetFeesToZero()
    {
        var searchModel = new SearchModel
        {
            Keywords = "test",
            NoFees = "true",
            FeeMin = 2m,
            FeeMax = 8m
        };

        var responseDto = new SearchResponse
        {
            Results = new List<SearchResultDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        _mockApiClient
            .Setup(x => x.SearchAsync(It.IsAny<SearchRequestDto>()))
            .ReturnsAsync(responseDto);

        var (results, totalCount) = await _searchService.SearchAsync(searchModel, 1, 10);

        results.Should().BeEmpty();
        totalCount.Should().Be(0);

        _mockApiClient.Verify(x => x.SearchAsync(It.Is<SearchRequestDto>(dto =>
            dto.MinFees == 0 && dto.MaxFees == 0
        )), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WhenNullDates_ShouldNotIncludeDateParameters()
    {
        var searchModel = new SearchModel
        {
            Keywords = "test"
        };

        var responseDto = new SearchResponse
        {
            Results = new List<SearchResultDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        _mockApiClient
            .Setup(x => x.SearchAsync(It.IsAny<SearchRequestDto>()))
            .ReturnsAsync(responseDto);

        var (results, _) = await _searchService.SearchAsync(searchModel, 1, 10);

        results.Should().BeEmpty();

        _mockApiClient.Verify(x => x.SearchAsync(It.Is<SearchRequestDto>(dto =>
            dto.SubmissionDeadlineFrom == null &&
            dto.SubmissionDeadlineTo == null
        )), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_ShouldMapStatusEnumCorrectly()
    {
        var searchModel = new SearchModel { Keywords = "test" };
        var responseDto = new SearchResponse
        {
            Results = new List<SearchResultDto>
            {
                new() { Id = "1", Title = "Active", Status = CommercialToolStatus.Active, MaximumFee = "Unknown", AwardMethod = "Open" },
                new() { Id = "2", Title = "Expired", Status = CommercialToolStatus.Expired, MaximumFee = "Unknown", AwardMethod = "Open" },
                new() { Id = "3", Title = "Upcoming", Status = CommercialToolStatus.Upcoming, MaximumFee = "Unknown", AwardMethod = "Open" },
                new() { Id = "4", Title = "Awarded", Status = CommercialToolStatus.Awarded, MaximumFee = "Unknown", AwardMethod = "Open" }
            },
            TotalCount = 4,
            PageNumber = 1,
            PageSize = 10
        };

        _mockApiClient
            .Setup(x => x.SearchAsync(It.IsAny<SearchRequestDto>()))
            .ReturnsAsync(responseDto);

        var (results, _) = await _searchService.SearchAsync(searchModel, 1, 10);

        results.Should().HaveCount(4);
        results.ElementAt(0).Status.Should().Be(CommercialToolStatus.Active);
        results.ElementAt(1).Status.Should().Be(CommercialToolStatus.Expired);
        results.ElementAt(2).Status.Should().Be(CommercialToolStatus.Upcoming);
        results.ElementAt(3).Status.Should().Be(CommercialToolStatus.Awarded);
    }

    [Fact]
    public async Task SearchAsync_ShouldFormatFeesAsCurrency()
    {
        var searchModel = new SearchModel { Keywords = "test" };
        var responseDto = new SearchResponse
        {
            Results = new List<SearchResultDto>
            {
                new()
                {
                    Id = "1",
                    Title = "Test",
                    Status = CommercialToolStatus.Active,
                    MaximumFee = "2.5%",
                    AwardMethod = "Open",
                    Description = "Test"
                }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        _mockApiClient
            .Setup(x => x.SearchAsync(It.IsAny<SearchRequestDto>()))
            .ReturnsAsync(responseDto);

        var (results, _) = await _searchService.SearchAsync(searchModel, 1, 10);

        results.Should().HaveCount(1);
        results.First().MaximumFee.Should().Be("2.5%");
    }

    [Fact]
    public async Task SearchAsync_ShouldFormatDatesCorrectly()
    {
        var searchModel = new SearchModel { Keywords = "test" };
        var submissionDeadline = new DateTime(2025, 3, 15);
        var responseDto = new SearchResponse
        {
            Results = new List<SearchResultDto>
            {
                new()
                {
                    Id = "1",
                    Title = "Test",
                    Status = CommercialToolStatus.Active,
                    MaximumFee = "Unknown",
                    AwardMethod = "Open",
                    Description = "Test",
                    SubmissionDeadline = submissionDeadline.ToString("dd MMMM yyyy")
                }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        _mockApiClient
            .Setup(x => x.SearchAsync(It.IsAny<SearchRequestDto>()))
            .ReturnsAsync(responseDto);

        var (results, _) = await _searchService.SearchAsync(searchModel, 1, 10);

        results.Should().HaveCount(1);
        results.First().SubmissionDeadline.Should().Be("15 March 2025");
    }

    [Theory]
    [InlineData("Sustainable Estates", KeywordSearchMode.Any, new[] { "Sustainable", "Estates" })]
    [InlineData("Sustainable+Estates", KeywordSearchMode.All, new[] { "Sustainable", "Estates" })]
    [InlineData("Sustainable + Estates", KeywordSearchMode.All, new[] { "Sustainable", "Estates" })]
    [InlineData("\"Sustainable Estates\"", KeywordSearchMode.Exact, new[] { "Sustainable Estates" })]
    [InlineData("technology+innovation+digital", KeywordSearchMode.All, new[] { "technology", "innovation", "digital" })]
    [InlineData("radio televisions", KeywordSearchMode.Any, new[] { "radio", "televisions" })]
    [InlineData("single", KeywordSearchMode.Any, new[] { "single" })]
    public async Task SearchAsync_ShouldParseKeywordsCorrectly(string input, KeywordSearchMode expectedMode, string[] expectedTerms)
    {
        var searchModel = new SearchModel { Keywords = input };
        var responseDto = new SearchResponse
        {
            Results = new List<SearchResultDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        _mockApiClient
            .Setup(x => x.SearchAsync(It.IsAny<SearchRequestDto>()))
            .ReturnsAsync(responseDto);

        await _searchService.SearchAsync(searchModel, 1, 10);

        _mockApiClient.Verify(x => x.SearchAsync(It.Is<SearchRequestDto>(dto =>
            dto.SearchMode == expectedMode &&
            dto.Keywords != null &&
            dto.Keywords.Count == expectedTerms.Length &&
            expectedTerms.All(term => dto.Keywords.Contains(term))
        )), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WhenKeywordsNull_ShouldPassNullKeywords()
    {
        var searchModel = new SearchModel { Keywords = null };
        var responseDto = new SearchResponse
        {
            Results = new List<SearchResultDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        _mockApiClient
            .Setup(x => x.SearchAsync(It.IsAny<SearchRequestDto>()))
            .ReturnsAsync(responseDto);

        await _searchService.SearchAsync(searchModel, 1, 10);

        _mockApiClient.Verify(x => x.SearchAsync(It.Is<SearchRequestDto>(dto =>
            dto.Keywords == null
        )), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WhenKeywordsEmpty_ShouldPassNullKeywords()
    {
        var searchModel = new SearchModel { Keywords = "" };
        var responseDto = new SearchResponse
        {
            Results = new List<SearchResultDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        _mockApiClient
            .Setup(x => x.SearchAsync(It.IsAny<SearchRequestDto>()))
            .ReturnsAsync(responseDto);

        await _searchService.SearchAsync(searchModel, 1, 10);

        _mockApiClient.Verify(x => x.SearchAsync(It.Is<SearchRequestDto>(dto =>
            dto.Keywords == null
        )), Times.Once);
    }
}