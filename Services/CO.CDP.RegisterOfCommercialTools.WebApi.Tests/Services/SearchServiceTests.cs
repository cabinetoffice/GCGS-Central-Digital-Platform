using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.RegisterOfCommercialTools.WebApi.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Tests.Services;

public class SearchServiceTests
{
    private readonly Mock<ICommercialToolsQueryBuilder> _mockQueryBuilder;
    private readonly Mock<ICommercialToolsRepository> _mockRepository;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly SearchService _searchService;

    public SearchServiceTests()
    {
        _mockQueryBuilder = new Mock<ICommercialToolsQueryBuilder>();
        _mockRepository = new Mock<ICommercialToolsRepository>();
        _mockConfiguration = new Mock<IConfiguration>();
        var mockBaseUrlSection = new Mock<IConfigurationSection>();
        mockBaseUrlSection.Setup(s => s.Value).Returns("https://test-api.example.com/v1/tender");
        _mockConfiguration.Setup(c => c.GetSection("ODataApi:BaseUrl")).Returns(mockBaseUrlSection.Object);
        _searchService = new SearchService(_mockQueryBuilder.Object, _mockRepository.Object, _mockConfiguration.Object);
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
            PageSize = 20,
            PageNumber = 2
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var finalBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords("IT services")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatus("Active")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.FeeFrom(100.50m)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.FeeTo(999.99m)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithPageSize(20)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithPageNumber(2)).Returns(mockBuilder.Object);
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
        _mockRepository.Setup(x => x.SearchCommercialTools(queryUrl)).ReturnsAsync(expectedResults);
        _mockRepository.Setup(x => x.GetCommercialToolsCount(queryUrl)).ReturnsAsync(100);

        var result = await _searchService.Search(request);

        result.Results.Should().BeEquivalentTo(expectedResults);
        result.TotalCount.Should().Be(100);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(20);
        _mockQueryBuilder.Verify(x => x.WithKeywords("IT services"), Times.Once);
        mockBuilder.Verify(x => x.WithStatus("Active"), Times.Once);
        mockBuilder.Verify(x => x.FeeFrom(100.50m), Times.Once);
        mockBuilder.Verify(x => x.FeeTo(999.99m), Times.Once);
        mockBuilder.Verify(x => x.WithPageSize(20), Times.Once);
        mockBuilder.Verify(x => x.WithPageNumber(2), Times.Once);
        mockBuilder.Verify(x => x.SubmissionDeadlineFrom(new DateTime(2025, 1, 1)), Times.Once);
        mockBuilder.Verify(x => x.SubmissionDeadlineTo(new DateTime(2025, 12, 31)), Times.Once);
        mockBuilder.Verify(x => x.ContractStartDateFrom(new DateTime(2025, 6, 1)), Times.Once);
        mockBuilder.Verify(x => x.ContractStartDateTo(new DateTime(2025, 6, 30)), Times.Once);
        mockBuilder.Verify(x => x.ContractEndDateFrom(new DateTime(2026, 1, 1)), Times.Once);
        mockBuilder.Verify(x => x.ContractEndDateTo(new DateTime(2026, 12, 31)), Times.Once);
        _mockRepository.Verify(x => x.SearchCommercialTools(queryUrl), Times.Once);
        _mockRepository.Verify(x => x.GetCommercialToolsCount(queryUrl), Times.Once);
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
            PageSize = 10,
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords("test")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatus("Active")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.FeeFrom(0)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.FeeTo(decimal.MaxValue)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithPageSize(10)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithPageNumber(1)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialTools(queryUrl)).ReturnsAsync(expectedResults);
        _mockRepository.Setup(x => x.GetCommercialToolsCount(queryUrl)).ReturnsAsync(0);

        var result = await _searchService.Search(request);

        result.Results.Should().BeEquivalentTo(expectedResults);
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);

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
            PageSize = 10,
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords("test")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatus("Active")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.FeeFrom(0)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.FeeTo(decimal.MaxValue)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithPageSize(10)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithPageNumber(1)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.SubmissionDeadlineFrom(new DateTime(2025, 1, 1))).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialTools(queryUrl)).ReturnsAsync(expectedResults);
        _mockRepository.Setup(x => x.GetCommercialToolsCount(queryUrl)).ReturnsAsync(0);

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
            PageSize = 10,
            PageNumber = 1
        };

        var mockBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var queryUrl = "https://api.example.com/tenders?built=query";

        _mockQueryBuilder.Setup(x => x.WithKeywords("")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithStatus("")).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.FeeFrom(0)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.FeeTo(decimal.MaxValue)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithPageSize(10)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.WithPageNumber(1)).Returns(mockBuilder.Object);
        mockBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(queryUrl);

        var expectedResults = new List<SearchResultDto>();
        _mockRepository.Setup(x => x.SearchCommercialTools(queryUrl)).ReturnsAsync(expectedResults);
        _mockRepository.Setup(x => x.GetCommercialToolsCount(queryUrl)).ReturnsAsync(0);

        var result = await _searchService.Search(request);

        result.Results.Should().BeEquivalentTo(expectedResults);
        result.TotalCount.Should().Be(0);
        _mockQueryBuilder.Verify(x => x.WithKeywords(""), Times.Once);
        mockBuilder.Verify(x => x.WithStatus(""), Times.Once);
    }

    [Fact]
    public void Constructor_WhenBaseUrlConfigurationIsMissing_ShouldThrowException()
    {
        var mockQueryBuilder = new Mock<ICommercialToolsQueryBuilder>();
        var mockRepository = new Mock<ICommercialToolsRepository>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockBaseUrlSection = new Mock<IConfigurationSection>();
        mockBaseUrlSection.Setup(s => s.Value).Returns((string?)null);
        mockConfiguration.Setup(c => c.GetSection("ODataApi:BaseUrl")).Returns(mockBaseUrlSection.Object);

        var action = () => new SearchService(mockQueryBuilder.Object, mockRepository.Object, mockConfiguration.Object);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("ODataApi:BaseUrl configuration is required");
    }
}