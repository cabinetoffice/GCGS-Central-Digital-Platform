using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.RegisterOfCommercialTools.WebApi.Services;
using FluentAssertions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Tests.Services;

public class CommercialToolsRepositoryTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly CommercialToolsRepository _repository;

    public CommercialToolsRepositoryTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com/")
        };
        var mockConfiguration = new Mock<IConfiguration>();
        var mockODataApiSection = new Mock<IConfigurationSection>();
        mockODataApiSection.Setup(s => s.Value).Returns("test-api-key");
        mockConfiguration.Setup(c => c.GetSection("ODataApi:ApiKey")).Returns(mockODataApiSection.Object);
        _repository = new CommercialToolsRepository(_httpClient, mockConfiguration.Object);
    }

    [Fact]
    public async Task SearchCommercialTools_ShouldReturnMappedResults()
    {
        var queryUrl = "https://api.example.com/tenders?filter=test";
        var jsonResponse = """
        [
            {
                "id": "123e4567-e89b-12d3-a456-426614174000",
                "releases": [
                    {
                        "id": "003033-2025"
                    }
                ],
                "title": "IT Services Framework",
                "description": "Framework for IT services",
                "link": "https://example.com/tender/1",
                "publishedDate": "2025-01-15T10:00:00Z",
                "tenderPeriod": {
                    "endDate": "2025-03-15T23:59:59Z"
                },
                "fees": 500.00,
                "awardMethod": "Direct Award",
                "status": "Active"
            }
        ]
        """;

        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var result = await _repository.SearchCommercialTools(queryUrl);

        var resultList = result.ToList();
        resultList.Should().HaveCount(1);

        var searchResult = resultList.First();
        searchResult.Id.Should().Be("003033-2025");
        searchResult.Title.Should().Be("IT Services Framework");
        searchResult.Description.Should().Be("Framework for IT services");
        searchResult.Link.Should().Be("https://example.com/tender/1");
        searchResult.PublishedDate.Should().Be(new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc));
        searchResult.SubmissionDeadline.Should().Be(new DateTime(2025, 3, 15, 23, 59, 59, DateTimeKind.Utc));
        searchResult.Fees.Should().Be(500.00m);
        searchResult.AwardMethod.Should().Be("Direct Award");
        searchResult.Status.Should().Be(CommercialToolStatus.Active);
    }

    [Fact]
    public async Task SearchCommercialTools_WhenReleasesEmpty_ShouldUseFallbackId()
    {
        var queryUrl = "https://api.example.com/tenders?filter=test";
        var jsonResponse = """
        [
            {
                "id": "123e4567-e89b-12d3-a456-426614174000",
                "releases": [],
                "title": "Test Framework",
                "description": "Test description",
                "link": "https://example.com/tender/1",
                "publishedDate": "2025-01-15T10:00:00Z",
                "fees": 0.00,
                "awardMethod": "Open",
                "status": "Active"
            }
        ]
        """;

        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var result = await _repository.SearchCommercialTools(queryUrl);

        var resultList = result.ToList();
        resultList.Should().HaveCount(1);
        resultList.First().Id.Should().Be("123e4567-e89b-12d3-a456-426614174000");
    }

    [Fact]
    public async Task SearchCommercialTools_WhenNoReleases_ShouldUseFallbackId()
    {
        var queryUrl = "https://api.example.com/tenders?filter=test";
        var jsonResponse = """
        [
            {
                "id": "123e4567-e89b-12d3-a456-426614174000",
                "title": "Test Framework",
                "description": "Test description",
                "link": "https://example.com/tender/1",
                "publishedDate": "2025-01-15T10:00:00Z",
                "fees": 0.00,
                "awardMethod": "Open",
                "status": "Active"
            }
        ]
        """;

        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var result = await _repository.SearchCommercialTools(queryUrl);

        var resultList = result.ToList();
        resultList.Should().HaveCount(1);
        resultList.First().Id.Should().Be("123e4567-e89b-12d3-a456-426614174000");
    }

    [Fact]
    public async Task SearchCommercialTools_ShouldMapStatusCorrectly()
    {
        var queryUrl = "https://api.example.com/tenders?filter=test";
        var jsonResponse = """
        [
            {
                "id": "123",
                "title": "Test",
                "description": "Test",
                "link": "https://example.com/1",
                "publishedDate": "2025-01-15T10:00:00Z",
                "fees": 0.00,
                "awardMethod": "Open",
                "status": "Closed"
            },
            {
                "id": "456",
                "title": "Test2",
                "description": "Test2",
                "link": "https://example.com/2",
                "publishedDate": "2025-01-15T10:00:00Z",
                "tenderPeriod": {
                    "endDate": "2026-01-15T23:59:59Z"
                },
                "fees": 0.00,
                "awardMethod": "Open",
                "status": "Open"
            }
        ]
        """;

        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var result = await _repository.SearchCommercialTools(queryUrl);

        var resultList = result.ToList();
        resultList.Should().HaveCount(2);

        resultList[0].Status.Should().Be(CommercialToolStatus.Closed);

        resultList[1].Status.Should().Be(CommercialToolStatus.Upcoming);
    }

    [Fact]
    public async Task SearchCommercialTools_WhenStatusIsUnknown_ShouldSetUnknownStatus()
    {
        var queryUrl = "https://api.example.com/tenders?filter=test";
        var jsonResponse = """
        [
            {
                "id": "123",
                "title": "Test",
                "description": "Test",
                "link": "https://example.com/1",
                "publishedDate": "2025-01-15T10:00:00Z",
                "fees": 0.00,
                "awardMethod": "Open",
                "status": "InvalidStatus"
            }
        ]
        """;

        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var result = await _repository.SearchCommercialTools(queryUrl);

        var resultList = result.ToList();
        resultList.Should().HaveCount(1);
        resultList.First().Status.Should().Be(CommercialToolStatus.Unknown);
    }

    [Fact]
    public async Task SearchCommercialTools_WhenHttpRequestFails_ShouldThrow()
    {
        var queryUrl = "https://api.example.com/tenders?filter=test";
        SetupHttpResponse(HttpStatusCode.InternalServerError, "Server Error");

        var action = async () => await _repository.SearchCommercialTools(queryUrl);
        await action.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetCommercialToolById_ShouldReturnSingleResult()
    {
        var id = "003033-2025";
        var jsonResponse = """
        {
            "id": "123e4567-e89b-12d3-a456-426614174000",
            "releases": [
                {
                    "id": "003033-2025"
                }
            ],
            "title": "Specific Framework",
            "description": "Specific framework description",
            "link": "https://example.com/tender/specific",
            "publishedDate": "2025-01-15T10:00:00Z",
            "tenderPeriod": {
                "endDate": "2025-03-15T23:59:59Z"
            },
            "fees": 750.00,
            "awardMethod": "Competitive",
            "status": "Active"
        }
        """;

        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var result = await _repository.GetCommercialToolById(id);

        result.Should().NotBeNull();
        result!.Id.Should().Be("003033-2025");
        result.Title.Should().Be("Specific Framework");
        result.Description.Should().Be("Specific framework description");
        result.Fees.Should().Be(750.00m);
        result.Status.Should().Be(CommercialToolStatus.Active);

        VerifyCorrectUrlWasCalled(HttpMethod.Get, $"CommercialTools({id})");
    }

    [Fact]
    public async Task GetCommercialToolById_WhenNotFound_ShouldThrow()
    {
        var id = "nonexistent-id";
        SetupHttpResponse(HttpStatusCode.NotFound, "Not Found");

        var result = await _repository.GetCommercialToolById(id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchCommercialTools_WhenEmptyResponse_ShouldReturnEmptyList()
    {
        var queryUrl = "https://api.example.com/tenders?filter=test";
        SetupHttpResponse(HttpStatusCode.OK, "[]");

        var result = await _repository.SearchCommercialTools(queryUrl);

        result.Should().BeEmpty();
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
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Headers.Contains("x-api-key") &&
                    req.Headers.GetValues("x-api-key").Contains("test-api-key")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    private void VerifyCorrectUrlWasCalled(HttpMethod method, string expectedUri)
    {
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == method &&
                    req.RequestUri!.ToString().EndsWith(expectedUri)),
                ItExpr.IsAny<CancellationToken>());
    }
}