using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.RegisterOfCommercialTools.WebApi.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
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
        _searchService = new SearchService(_mockQueryBuilder.Object, _mockRepository.Object, mockConfiguration.Object);
    }

    [Fact]
    public async Task Search_ShouldBuildQueryWithAllParameters()
    {
        var request = new SearchRequestDto
        {
            Keyword = "IT services",
            Status = "Active",
            SubmissionDeadlineFrom = new DateTime(2025, 1, 1),
            SubmissionDeadlineTo = new DateTime(2025, 12, 31),
            ContractStartDateFrom = new DateTime(2025, 6, 1),
            ContractStartDateTo = new DateTime(2025, 6, 30),
            ContractEndDateFrom = new DateTime(2026, 1, 1),
            ContractEndDateTo = new DateTime(2026, 12, 31),
            MinFees = 100.50m,
            MaxFees = 999.99m,
            PageNumber = 2
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var finalBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords("IT services")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCustomFilter(It.IsAny<string>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.SubmissionDeadlineFrom(new DateTime(2025, 1, 1))).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.SubmissionDeadlineTo(new DateTime(2025, 12, 31))).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.ContractStartDateFrom(new DateTime(2025, 6, 1))).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.ContractStartDateTo(new DateTime(2025, 6, 30))).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.ContractEndDateFrom(new DateTime(2026, 1, 1))).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.ContractEndDateTo(new DateTime(2026, 12, 31))).Returns(finalBuilder.Object);
        finalBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>
        {
            new() { Id = "003033-2025", Title = "Test Result" }
        };
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync((expectedResults, 100));

        var result = await _searchService.Search(request);

        result.Results.Should().BeEquivalentTo(expectedResults);
        result.TotalCount.Should().Be(100);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(20);
        _mockQueryBuilder.Verify(x => x.WithKeywords("IT services"), Times.Once);
        mockBuilder.Verify(x => x.WithCustomFilter(It.Is<string>(s => s.Contains("tender/status eq 'active'"))), Times.Once);
        mockBuilder.Verify(x => x.WithCustomFilter(It.Is<string>(s => s.Contains("participationFees") && s.Contains("proportion"))), Times.Once);
        mockBuilder.Verify(x => x.WithTop(20), Times.Once);
        mockBuilder.Verify(x => x.WithSkip(20), Times.Once);
        mockBuilder.Verify(x => x.SubmissionDeadlineFrom(new DateTime(2025, 1, 1)), Times.Once);
        mockBuilder.Verify(x => x.SubmissionDeadlineTo(new DateTime(2025, 12, 31)), Times.Once);
        mockBuilder.Verify(x => x.ContractStartDateFrom(new DateTime(2025, 6, 1)), Times.Once);
        mockBuilder.Verify(x => x.ContractStartDateTo(new DateTime(2025, 6, 30)), Times.Once);
        mockBuilder.Verify(x => x.ContractEndDateFrom(new DateTime(2026, 1, 1)), Times.Once);
        mockBuilder.Verify(x => x.ContractEndDateTo(new DateTime(2026, 12, 31)), Times.Once);
        _mockRepository.Verify(x => x.SearchCommercialToolsWithCount(queryUrl), Times.Once);
    }

    [Fact]
    public async Task Search_WhenOptionalDatesAreNull_ShouldNotAddDateFilters()
    {
        var request = new SearchRequestDto
        {
            Keyword = "test",
            Status = "Active",
            MinFees = 0,
            MaxFees = decimal.MaxValue,
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords("test")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.FeeFrom(0)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.FeeTo(decimal.MaxValue)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCustomFilter(It.IsAny<string>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync((expectedResults, 0));

        var result = await _searchService.Search(request);

        result.Results.Should().BeEquivalentTo(expectedResults);
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(20);

        mockBuilder.Verify(x => x.SubmissionDeadlineFrom(It.IsAny<DateTime>()), Times.Never);
        mockBuilder.Verify(x => x.SubmissionDeadlineTo(It.IsAny<DateTime>()), Times.Never);
        mockBuilder.Verify(x => x.ContractStartDateFrom(It.IsAny<DateTime>()), Times.Never);
        mockBuilder.Verify(x => x.ContractStartDateTo(It.IsAny<DateTime>()), Times.Never);
        mockBuilder.Verify(x => x.ContractEndDateFrom(It.IsAny<DateTime>()), Times.Never);
        mockBuilder.Verify(x => x.ContractEndDateTo(It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task Search_WhenOnlySubmissionDeadlineFromProvided_ShouldOnlyAddSubmissionDeadlineFromFilter()
    {
        var request = new SearchRequestDto
        {
            Keyword = "test",
            Status = "Active",
            SubmissionDeadlineFrom = new DateTime(2025, 1, 1),
            MinFees = 0,
            MaxFees = decimal.MaxValue,
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords("test")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.FeeFrom(0)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.FeeTo(decimal.MaxValue)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCustomFilter(It.IsAny<string>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.SubmissionDeadlineFrom(new DateTime(2025, 1, 1))).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync((expectedResults, 0));

        var result = await _searchService.Search(request);

        result.Results.Should().BeEquivalentTo(expectedResults);
        result.TotalCount.Should().Be(0);
        mockBuilder.Verify(x => x.SubmissionDeadlineFrom(new DateTime(2025, 1, 1)), Times.Once);

        mockBuilder.Verify(x => x.SubmissionDeadlineTo(It.IsAny<DateTime>()), Times.Never);
        mockBuilder.Verify(x => x.ContractStartDateFrom(It.IsAny<DateTime>()), Times.Never);
        mockBuilder.Verify(x => x.ContractStartDateTo(It.IsAny<DateTime>()), Times.Never);
        mockBuilder.Verify(x => x.ContractEndDateFrom(It.IsAny<DateTime>()), Times.Never);
        mockBuilder.Verify(x => x.ContractEndDateTo(It.IsAny<DateTime>()), Times.Never);
    }


    [Fact]
    public async Task Search_WithEmptyKeyword_ShouldPassEmptyStringToQueryBuilder()
    {
        var request = new SearchRequestDto
        {
            Keyword = null,
            Status = null,
            MinFees = 0,
            MaxFees = decimal.MaxValue,
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords("")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.FeeFrom(0)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.FeeTo(decimal.MaxValue)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithCustomFilter(It.IsAny<string>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync((expectedResults, 0));

        var result = await _searchService.Search(request);

        result.Results.Should().BeEquivalentTo(expectedResults);
        result.TotalCount.Should().Be(0);
        _mockQueryBuilder.Verify(x => x.WithKeywords(""), Times.Once);
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

        var action = () => new SearchService(mockQueryBuilder.Object, mockRepository.Object, mockConfiguration.Object);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("ODataApi:BaseUrl configuration is required");
    }

    [Fact]
    public async Task Search_WhenSingleAwardMethodProvided_ShouldPassToQueryBuilder()
    {
        var request = new SearchRequestDto
        {
            Keyword = "test",
            AwardMethod = ["with-competition"],
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords("test")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethod("with-competition")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync((expectedResults, 0));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithAwardMethod("with-competition"), Times.Once);
    }

    [Fact]
    public async Task Search_WhenBothAwardMethodsProvided_ShouldPassCombinedValue()
    {
        var request = new SearchRequestDto
        {
            Keyword = "test",
            AwardMethod = ["with-competition", "without-competition"],
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords("test")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithAwardMethod("with-and-without-competition")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync((expectedResults, 0));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithAwardMethod("with-and-without-competition"), Times.Once);
    }

    [Fact]
    public async Task Search_WhenNoAwardMethodProvided_ShouldNotCallWithAwardMethod()
    {
        var request = new SearchRequestDto
        {
            Keyword = "test",
            AwardMethod = null,
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords("test")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync((expectedResults, 0));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithAwardMethod(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Search_WhenEmptyAwardMethodProvided_ShouldNotCallWithAwardMethod()
    {
        var request = new SearchRequestDto
        {
            Keyword = "test",
            AwardMethod = [],
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords("test")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithTop(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithSkip(It.IsAny<int>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialToolsWithCount(queryUrl)).ReturnsAsync((expectedResults, 0));

        await _searchService.Search(request);

        mockBuilder.Verify(x => x.WithAwardMethod(It.IsAny<string>()), Times.Never);
    }
}