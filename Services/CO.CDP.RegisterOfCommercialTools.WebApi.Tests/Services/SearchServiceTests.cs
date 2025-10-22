using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.RegisterOfCommercialTools.WebApi.Services;
using CO.CDP.WebApi.Foundation;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Tests.Services;

public class SearchServiceTests
{
    private readonly Mock<ICommercialToolsQueryBuilder> _mockQueryBuilder;
    private readonly Mock<ICommercialToolsService> _mockRepository;
    private readonly SearchService _searchService;

    public SearchServiceTests()
    {
        _mockQueryBuilder = new Mock<ICommercialToolsQueryBuilder>();
        _mockRepository = new Mock<ICommercialToolsService>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockBaseUrlSection = new Mock<IConfigurationSection>();
        mockBaseUrlSection.Setup(s => s.Value).Returns("https://test-api.example.com/v1/tender");
        mockConfiguration.Setup(c => c.GetSection("ODataApi:BaseUrl")).Returns(mockBaseUrlSection.Object);
        var mockLogger = new Mock<ILogger<SearchService>>();
        _searchService = new SearchService(_mockQueryBuilder.Object, _mockRepository.Object, mockConfiguration.Object, mockLogger.Object);
    }

    [Fact]
    public async Task Search_ShouldBuildQueryWithAllParameters()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["IT", "services"],
            SearchMode = KeywordSearchMode.Any,
            Status = "Active",
            SubmissionDeadlineFrom = new DateTime(2025, 1, 1),
            SubmissionDeadlineTo = new DateTime(2025, 12, 31),
            ContractStartDate = new DateTime(2025, 6, 1),
            ContractEndDate = new DateTime(2026, 12, 31),
            MinFees = 100.50m,
            MaxFees = 999.99m,
            PageNumber = 2
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatuses(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCpvCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithLocationCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethods(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.FeeFrom(It.IsAny<decimal>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.FeeTo(It.IsAny<decimal>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCustomFilter(It.IsAny<string>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.SubmissionDeadlineFrom(new DateTime(2025, 1, 1))).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.SubmissionDeadlineTo(new DateTime(2025, 12, 31))).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.ContractStartDate(new DateTime(2025, 6, 1))).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.ContractEndDate(new DateTime(2026, 12, 31))).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethods(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCpvCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithLocationCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>
        {
            new() { Id = "003033-2025", Title = "Test Result" }
        };
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync(ApiResult<(IEnumerable<SearchResultDto>, int)>.Success((expectedResults, 100)));

        var apiResult = await _searchService.Search(request);
        var result = apiResult.Match(
            error => { Assert.Fail($"Expected success but got error: {error}"); return default!; },
            value => value
        );

        result.Results.Should().BeEquivalentTo(expectedResults);
        result.TotalCount.Should().Be(100);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(20);
        _mockQueryBuilder.Verify(x => x.WithKeywords(It.IsAny<List<string>>(), KeywordSearchMode.Any), Times.Once);
        mockBuilder.Verify(x => x.WithStatuses(It.Is<List<string>>(l => l.Contains("Active"))), Times.Once);
        mockBuilder.Verify(x => x.WithTop(20), Times.Once);
        mockBuilder.Verify(x => x.WithSkip(20), Times.Once);
        mockBuilder.Verify(x => x.SubmissionDeadlineFrom(new DateTime(2025, 1, 1)), Times.Once);
        mockBuilder.Verify(x => x.SubmissionDeadlineTo(new DateTime(2025, 12, 31)), Times.Once);
        mockBuilder.Verify(x => x.ContractStartDate(new DateTime(2025, 6, 1)), Times.Once);
        mockBuilder.Verify(x => x.ContractEndDate(new DateTime(2026, 12, 31)), Times.Once);
        _mockRepository.Verify(x => x.SearchCommercialToolsWithCount(queryUrl), Times.Once);
    }

    [Fact]
    public async Task Search_WhenOptionalDatesAreNull_ShouldNotAddDateFilters()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            Status = "Active",
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCustomFilter(It.IsAny<string>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatuses(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCpvCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithLocationCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethods(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync(ApiResult<(IEnumerable<SearchResultDto>, int)>.Success((expectedResults, 0)));

        var apiResult = await _searchService.Search(request);
        var result = apiResult.Match(
            error => { Assert.Fail($"Expected success but got error: {error}"); return default!; },
            value => value
        );

        result.Results.Should().BeEquivalentTo(expectedResults);
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(20);

        mockBuilder.Verify(x => x.SubmissionDeadlineFrom(It.IsAny<DateTime>()), Times.Never);
        mockBuilder.Verify(x => x.SubmissionDeadlineTo(It.IsAny<DateTime>()), Times.Never);
        mockBuilder.Verify(x => x.ContractStartDate(It.IsAny<DateTime>()), Times.Never);
        mockBuilder.Verify(x => x.ContractEndDate(It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task Search_WhenOnlySubmissionDeadlineFromProvided_ShouldOnlyAddSubmissionDeadlineFromFilter()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            Status = "Active",
            SubmissionDeadlineFrom = new DateTime(2025, 1, 1),
            MinFees = 0,
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCustomFilter(It.IsAny<string>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatuses(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCpvCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithLocationCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethods(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.SubmissionDeadlineFrom(new DateTime(2025, 1, 1))).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync(ApiResult<(IEnumerable<SearchResultDto>, int)>.Success((expectedResults, 0)));

        var apiResult = await _searchService.Search(request);
        var result = apiResult.Match(
            error => { Assert.Fail($"Expected success but got error: {error}"); return default!; },
            value => value
        );

        result.Results.Should().BeEquivalentTo(expectedResults);
        result.TotalCount.Should().Be(0);
        mockBuilder.Verify(x => x.SubmissionDeadlineFrom(new DateTime(2025, 1, 1)), Times.Once);

        mockBuilder.Verify(x => x.SubmissionDeadlineTo(It.IsAny<DateTime>()), Times.Never);
        mockBuilder.Verify(x => x.ContractStartDate(It.IsAny<DateTime>()), Times.Never);
        mockBuilder.Verify(x => x.ContractEndDate(It.IsAny<DateTime>()), Times.Never);
    }


    [Fact]
    public async Task Search_WithEmptyKeywords_ShouldPassEmptyStringToQueryBuilder()
    {
        var request = new SearchRequestDto
        {
            Keywords = null,
            Status = null,
            MinFees = 0,
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCustomFilter(It.IsAny<string>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatuses(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCpvCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithLocationCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethods(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync(ApiResult<(IEnumerable<SearchResultDto>, int)>.Success((expectedResults, 0)));

        var apiResult = await _searchService.Search(request);
        var result = apiResult.Match(
            error => { Assert.Fail($"Expected success but got error: {error}"); return default!; },
            value => value
        );

        result.Results.Should().BeEquivalentTo(expectedResults);
        result.TotalCount.Should().Be(0);
        _mockQueryBuilder.Verify(x => x.WithKeywords(null, It.IsAny<KeywordSearchMode>()), Times.Once);
    }

    [Fact]
    public void Constructor_WhenBaseUrlConfigurationIsMissing_ShouldThrowException()
    {
        var mockQueryBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var mockRepository = new Mock<ICommercialToolsService>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockBaseUrlSection = new Mock<IConfigurationSection>();
        mockBaseUrlSection.Setup(s => s.Value).Returns((string?)null);
        mockConfiguration.Setup(c => c.GetSection("ODataApi:BaseUrl")).Returns(mockBaseUrlSection.Object);
        var mockLogger = new Mock<ILogger<SearchService>>();

        var action = () => new SearchService(mockQueryBuilder.Object, mockRepository.Object, mockConfiguration.Object, mockLogger.Object);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("ODataApi:BaseUrl configuration is required");
    }

    [Fact]
    public async Task Search_WhenSingleAwardMethodProvided_ShouldPassToQueryBuilder()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            AwardMethod = ["with-competition"],
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatuses(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCpvCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithLocationCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethods(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethods(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync(ApiResult<(IEnumerable<SearchResultDto>, int)>.Success((expectedResults, 0)));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithAwardMethods(It.Is<List<string>>(l => l.Count == 1 && l[0] == "with-competition")), Times.Once);
    }

    [Fact]
    public async Task Search_WhenBothAwardMethodsProvided_ShouldPassCombinedValue()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            AwardMethod = ["with-competition", "without-competition"],
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatuses(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCpvCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithLocationCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethods(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync(ApiResult<(IEnumerable<SearchResultDto>, int)>.Success((expectedResults, 0)));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithAwardMethods(It.Is<List<string>>(l => l.Count == 2 && l.Contains("with-competition") && l.Contains("without-competition"))), Times.Once);
    }

    [Fact]
    public async Task Search_WhenNoAwardMethodProvided_ShouldCallWithNull()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            AwardMethod = null,
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatuses(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCpvCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithLocationCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethods(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync(ApiResult<(IEnumerable<SearchResultDto>, int)>.Success((expectedResults, 0)));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithAwardMethods(null), Times.Once);
    }

    [Fact]
    public async Task Search_WhenEmptyAwardMethodProvided_ShouldCallWithEmptyList()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            AwardMethod = [],
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatuses(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCpvCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithLocationCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethods(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync(ApiResult<(IEnumerable<SearchResultDto>, int)>.Success((expectedResults, 0)));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithAwardMethods(It.Is<List<string>>(l => l.Count == 0)), Times.Once);
    }

    [Fact]
    public async Task Search_WhenSingleCpvCodeProvided_ShouldAddCpvFilter()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            CpvCodes = ["12345678"],
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatuses(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCpvCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithLocationCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethods(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync(ApiResult<(IEnumerable<SearchResultDto>, int)>.Success((expectedResults, 0)));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithCpvCodes(It.Is<List<string>>(l => l.Contains("12345678"))), Times.Once);
    }

    [Fact]
    public async Task Search_WhenMultipleCpvCodesProvided_ShouldCombineWithOr()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            CpvCodes = ["12345678", "87654321"],
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatuses(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCpvCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithLocationCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethods(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync(ApiResult<(IEnumerable<SearchResultDto>, int)>.Success((expectedResults, 0)));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithCpvCodes(It.Is<List<string>>(l => l.Contains("12345678") && l.Contains("87654321"))), Times.Once);
    }

    [Fact]
    public async Task Search_WhenNoCpvCodesProvided_ShouldNotAddCpvFilter()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            CpvCodes = null,
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatuses(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCpvCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithLocationCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethods(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync(ApiResult<(IEnumerable<SearchResultDto>, int)>.Success((expectedResults, 0)));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithCustomFilter(It.Is<string>(f =>
            f.Contains("tender/classification") && f.Contains("CPV"))), Times.Never);
    }

    [Fact]
    public async Task Search_WhenSingleLocationCodeProvided_ShouldAddLocationFilter()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            LocationCodes = ["UKN06"],
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatuses(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCpvCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithLocationCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethods(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync(ApiResult<(IEnumerable<SearchResultDto>, int)>.Success((expectedResults, 0)));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithLocationCodes(It.Is<List<string>>(l => l.Contains("UKN06"))), Times.Once);
    }

    [Fact]
    public async Task Search_WhenMultipleLocationCodesProvided_ShouldCombineWithOr()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            LocationCodes = ["UKN06", "UKC", "UKD"],
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatuses(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCpvCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithLocationCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethods(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync(ApiResult<(IEnumerable<SearchResultDto>, int)>.Success((expectedResults, 0)));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithLocationCodes(It.Is<List<string>>(l =>
            l.Count == 3 && l.Contains("UKN06") && l.Contains("UKC") && l.Contains("UKD"))), Times.Once);
    }

    [Fact]
    public async Task Search_WhenNoLocationCodesProvided_ShouldNotAddLocationFilter()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            LocationCodes = null,
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatuses(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCpvCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithLocationCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethods(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync(ApiResult<(IEnumerable<SearchResultDto>, int)>.Success((expectedResults, 0)));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithCustomFilter(It.Is<string>(f =>
            f.Contains("deliveryAddresses") && f.Contains("region"))), Times.Never);
    }

    [Fact]
    public async Task Search_WhenFilterFrameworksIsTrue_ShouldCallWithFrameworkAgreement()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            FilterFrameworks = true,
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatuses(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCpvCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithLocationCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethods(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithFrameworkAgreement(true)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync(ApiResult<(IEnumerable<SearchResultDto>, int)>.Success((expectedResults, 0)));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithFrameworkAgreement(true), Times.Once);
    }

    [Fact]
    public async Task Search_WhenIsOpenFrameworksIsTrue_ShouldCallOnlyOpenFrameworks()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            IsOpenFrameworks = true,
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatuses(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCpvCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithLocationCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethods(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.OnlyOpenFrameworks(true)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync(ApiResult<(IEnumerable<SearchResultDto>, int)>.Success((expectedResults, 0)));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.OnlyOpenFrameworks(true), Times.Once);
    }

    [Fact]
    public async Task Search_WhenFilterDynamicMarketsIsTrue_ShouldCallWithDynamicPurchasingSystem()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            FilterDynamicMarkets = true,
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatuses(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCpvCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithLocationCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethods(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithDynamicPurchasingSystem(true)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync(ApiResult<(IEnumerable<SearchResultDto>, int)>.Success((expectedResults, 0)));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithDynamicPurchasingSystem(true), Times.Once);
    }

    [Fact]
    public async Task Search_WhenIsUtilitiesOnlyIsTrue_ShouldCallWithBuyerClassificationRestrictions()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            IsUtilitiesOnly = true,
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatuses(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCpvCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithLocationCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethods(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithBuyerClassificationRestrictions("utilities")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync(ApiResult<(IEnumerable<SearchResultDto>, int)>.Success((expectedResults, 0)));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithBuyerClassificationRestrictions("utilities"), Times.Once);
    }

    [Fact]
    public async Task Search_WhenAllBooleanFiltersAreTrue_ShouldApplyAllFilters()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            FilterFrameworks = true,
            IsOpenFrameworks = true,
            FilterDynamicMarkets = true,
            IsUtilitiesOnly = true,
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatuses(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCpvCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithLocationCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethods(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCustomFilter(It.IsAny<string>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.OnlyOpenFrameworks(true)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithBuyerClassificationRestrictions("utilities")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync(ApiResult<(IEnumerable<SearchResultDto>, int)>.Success((expectedResults, 0)));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithCustomFilter(It.Is<string>(s => s.Contains("hasFrameworkAgreement eq true or") && s.Contains("hasDynamicPurchasingSystem eq true"))), Times.Once);
        mockBuilder.Verify(x => x.OnlyOpenFrameworks(true), Times.Once);
        mockBuilder.Verify(x => x.WithBuyerClassificationRestrictions("utilities"), Times.Once);
    }

    [Fact]
    public async Task Search_WhenOnlyFilterFrameworks_ShouldApplyFrameworkFilter()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            FilterFrameworks = true,
            FilterDynamicMarkets = false,
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatuses(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCpvCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithLocationCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethods(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithFrameworkAgreement(true)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync(ApiResult<(IEnumerable<SearchResultDto>, int)>.Success((expectedResults, 0)));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithFrameworkAgreement(true), Times.Once);
        mockBuilder.Verify(x => x.WithDynamicPurchasingSystem(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Search_WhenOnlyFilterDynamicMarkets_ShouldApplyDynamicMarketFilter()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            FilterFrameworks = false,
            FilterDynamicMarkets = true,
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatuses(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCpvCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithLocationCodes(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethods(It.IsAny<List<string>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithDynamicPurchasingSystem(true)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync(ApiResult<(IEnumerable<SearchResultDto>, int)>.Success((expectedResults, 0)));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithDynamicPurchasingSystem(true), Times.Once);
        mockBuilder.Verify(x => x.WithFrameworkAgreement(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Search_WhenContractingAuthorityUsageIsYes_ShouldApplyOpenFrameworkFilter()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            ContractingAuthorityUsage = ["yes"],
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithFrameworkType("open")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync((expectedResults, 0));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithFrameworkType("open"), Times.Once);
    }

    [Fact]
    public async Task Search_WhenContractingAuthorityUsageIsNo_ShouldApplyClosedFrameworkFilter()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            ContractingAuthorityUsage = ["no"],
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithFrameworkType("closed")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync((expectedResults, 0));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithFrameworkType("closed"), Times.Once);
    }

    [Fact]
    public async Task Search_WhenContractingAuthorityUsageIsBoth_ShouldNotApplyFrameworkFilter()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            ContractingAuthorityUsage = ["yes", "no"],
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync((expectedResults, 0));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithFrameworkType(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Search_WhenContractingAuthorityUsageIsEmpty_ShouldNotApplyFrameworkFilter()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            ContractingAuthorityUsage = [],
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords(It.IsAny<List<string>>(), It.IsAny<KeywordSearchMode>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithOrderBy("relevance")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync((expectedResults, 0));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithFrameworkType(It.IsAny<string>()), Times.Never);
    }
}