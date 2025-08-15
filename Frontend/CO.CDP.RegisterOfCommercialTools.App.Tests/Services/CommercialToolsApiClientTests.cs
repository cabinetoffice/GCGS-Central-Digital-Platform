using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.App.Services;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using FluentAssertions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;

namespace CO.CDP.RegisterOfCommercialTools.App.Tests.Services;

public class CommercialToolsApiClientTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly CommercialToolsApiClient _apiClient;

    public CommercialToolsApiClientTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com/")
        };
        _apiClient = new CommercialToolsApiClient(_httpClient);
    }

    [Fact]
    public async Task SearchAsync_ShouldMapSearchModelToRequestDto()
    {
        var searchModel = new SearchModel
        {
            Keywords = "IT services",
            Status = ["Active"],
            FeeMin = 100.50m,
            FeeMax = 999.99m,
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
                    Link = "https://test.com/framework",
                    PublishedDate = DateTime.UtcNow,
                    SubmissionDeadline = DateTime.UtcNow.AddDays(30),
                    Status = CommercialToolStatus.Active,
                    Fees = 500.00m,
                    AwardMethod = "Competitive"
                }
            },
            TotalCount = 25,
            PageNumber = 1,
            PageSize = 10
        };

        var jsonResponse = JsonSerializer.Serialize(responseDto);
        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var (results, totalCount) = await _apiClient.SearchAsync(searchModel, 1, 10);

        results.Should().HaveCount(1);
        var result = results.First();
        result.Id.Should().Be("003033-2025");
        result.Title.Should().Be("Test Framework");
        result.Caption.Should().Be("Test Description");
        result.Status.Should().Be(SearchResultStatus.Active);
        totalCount.Should().Be(25);

        VerifyHttpRequest(HttpMethod.Get, req =>
        {
            var query = req.RequestUri!.Query;
            return query.Contains("Keyword=IT%20services") &&

                   query.Contains("MinFees=100.5") &&
                   query.Contains("MaxFees=999.99") &&
                   query.Contains("AwardMethod=Competitive") &&
                   query.Contains("SubmissionDeadlineFrom=") &&
                   query.Contains("SubmissionDeadlineTo=") &&
                   query.Contains("ContractStartDateFrom=") &&
                   query.Contains("ContractStartDateTo=") &&
                   query.Contains("PageNumber=1") &&
                   query.Contains("PageSize=10");
        });
    }

    [Fact]
    public async Task SearchAsync_WhenNoFeesSelected_ShouldSetFeesToZero()
    {
        var searchModel = new SearchModel
        {
            Keywords = "test",
            NoFees = "true",
            FeeMin = 100m,
            FeeMax = 500m
        };

        var responseDto = new SearchResponse
        {
            Results = new List<SearchResultDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };
        var jsonResponse = JsonSerializer.Serialize(responseDto);
        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var (results, totalCount) = await _apiClient.SearchAsync(searchModel, 1, 10);

        results.Should().BeEmpty();
        totalCount.Should().Be(0);

        VerifyHttpRequest(HttpMethod.Get, req =>
        {
            var query = req.RequestUri!.Query;
            return query.Contains("MinFees=0") && query.Contains("MaxFees=0");
        });
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
        var jsonResponse = JsonSerializer.Serialize(responseDto);
        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var (results, totalCount) = await _apiClient.SearchAsync(searchModel, 1, 10);

        results.Should().BeEmpty();

        VerifyHttpRequest(HttpMethod.Get, req =>
        {
            var query = req.RequestUri!.Query;
            return !query.Contains("SubmissionDeadlineFrom=") ||
                   query.Contains("SubmissionDeadlineFrom=&");
        });
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

        var jsonResponse = JsonSerializer.Serialize(responseDto);
        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var (results, _) = await _apiClient.SearchAsync(searchModel, 1, 10);

        results.Should().HaveCount(4);
        results.ElementAt(0).Status.Should().Be(SearchResultStatus.Active);
        results.ElementAt(1).Status.Should().Be(SearchResultStatus.Closed);
        results.ElementAt(2).Status.Should().Be(SearchResultStatus.Upcoming);
        results.ElementAt(3).Status.Should().Be(SearchResultStatus.Awarded);
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
                    Fees = 1250.75m,
                    AwardMethod = "Open",
                    Description = "Test",
                    Link = "https://test.com"
                }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        var jsonResponse = JsonSerializer.Serialize(responseDto);
        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var (results, _) = await _apiClient.SearchAsync(searchModel, 1, 10);

        results.Should().HaveCount(1);
        results.First().MaximumFee.Should().Be("£1,250.75");
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
                    Link = "https://test.com",
                    SubmissionDeadline = submissionDeadline
                }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        var jsonResponse = JsonSerializer.Serialize(responseDto);
        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var (results, _) = await _apiClient.SearchAsync(searchModel, 1, 10);

        results.Should().HaveCount(1);
        results.First().SubmissionDeadline.Should().Be(submissionDeadline.ToShortDateString());
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    private void VerifyHttpRequest(HttpMethod method, Func<HttpRequestMessage, bool> requestValidator)
    {
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == method && requestValidator(req)),
                ItExpr.IsAny<CancellationToken>());
    }
}