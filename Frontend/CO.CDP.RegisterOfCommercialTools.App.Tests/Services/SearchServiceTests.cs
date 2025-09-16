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
            AwardMethod = "Competitive"
        };

        searchModel.SubmissionDeadline.From.Day = "1";
        searchModel.SubmissionDeadline.From.Month = "1";
        searchModel.SubmissionDeadline.From.Year = "2025";
        searchModel.SubmissionDeadline.To.Day = "31";
        searchModel.SubmissionDeadline.To.Month = "12";
        searchModel.SubmissionDeadline.To.Year = "2025";

        searchModel.ContractStartDate.From.Day = "1";
        searchModel.ContractStartDate.From.Month = "6";
        searchModel.ContractStartDate.From.Year = "2025";
        searchModel.ContractStartDate.To.Day = "30";
        searchModel.ContractStartDate.To.Month = "6";
        searchModel.ContractStartDate.To.Year = "2025";

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
                    SubmissionDeadline = DateTime.UtcNow.AddDays(30),
                    Status = CommercialToolStatus.Active,
                    Fees = 0.025m,
                    AwardMethod = "Competitive"
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
            dto.Keyword == "IT services" &&
            dto.MinFees == 0.01m &&
            dto.MaxFees == 0.05m &&
            dto.AwardMethod == "Competitive" &&
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
                new() { Id = "1", Title = "Active", Status = CommercialToolStatus.Active, Fees = 0, AwardMethod = "Open" },
                new() { Id = "2", Title = "Closed", Status = CommercialToolStatus.Closed, Fees = 0, AwardMethod = "Open" },
                new() { Id = "3", Title = "Upcoming", Status = CommercialToolStatus.Upcoming, Fees = 0, AwardMethod = "Open" },
                new() { Id = "4", Title = "Awarded", Status = CommercialToolStatus.Awarded, Fees = 0, AwardMethod = "Open" }
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
        results.ElementAt(1).Status.Should().Be(CommercialToolStatus.Closed);
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
                    Fees = 0.025m,
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
                    Fees = 0,
                    AwardMethod = "Open",
                    Description = "Test",
                    SubmissionDeadline = submissionDeadline
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
        results.First().SubmissionDeadline.Should().Be(submissionDeadline.ToShortDateString());
    }
}