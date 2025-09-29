using AutoMapper;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.RegisterOfCommercialTools.WebApi.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Tests.Services;

public class CommercialToolsServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CommercialToolsService _service;

    public CommercialToolsServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com/")
        };
        var mockConfiguration = new Mock<IConfiguration>();
        var mockODataApiSection = new Mock<IConfigurationSection>();
        mockODataApiSection.Setup(s => s.Value).Returns("test-api-key");
        mockConfiguration.Setup(c => c.GetSection("ODataApi:ApiKey")).Returns(mockODataApiSection.Object);

        var mockLogger = new Mock<ILogger<CommercialToolsService>>();
        _mockMapper = new Mock<IMapper>();
        _service = new CommercialToolsService(httpClient, mockConfiguration.Object, mockLogger.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task SearchCommercialToolsWithCount_ShouldReturnMappedResults()
    {
        var queryUrl = "https://api.example.com/tenders?filter=test";
        var jsonResponse = """
        {
            "success": true,
            "statusCode": 200,
            "message": "Commercial Tools retrieved successfully",
            "data": [
                {
                    "tenderId": "da77fe43-bc0a-43fe-b05d-8c292833404b",
                    "tenderIdentifier": "ocds-h6vhtk-04f907",
                    "title": "IT Services Framework",
                    "status": "active",
                    "description": "Test procurement description",
                    "createdAt": {
                        "value": "2025-01-15T10:00:00Z"
                    },
                    "tenderPeriod": {
                        "endDate": "2025-03-15T23:59:59Z"
                    },
                    "techniques": {
                        "hasFrameworkAgreement": true,
                        "frameworkAgreement": {
                            "method": "open",
                            "isOpenFrameworkScheme": true
                        }
                    }
                }
            ],
            "paging": {
                "totalItems": 1,
                "currentPage": 1,
                "pageSize": 10
            }
        }
        """;

        var expectedSearchResult = new SearchResultDto
        {
            Id = "ocds-h6vhtk-04f907",
            Title = "IT Services Framework",
            Description = "Test procurement description",
            PublishedDate = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            SubmissionDeadline = new DateTime(2025, 3, 15, 23, 59, 59, DateTimeKind.Utc),
            Fees = 0,
            AwardMethod = "With competition",
            Status = CommercialToolStatus.Active,
            OtherContractingAuthorityCanUse = "Yes"
        };

        _mockMapper.Setup(m => m.Map<SearchResultDto>(It.IsAny<CommercialToolApiItem>()))
            .Returns(expectedSearchResult);

        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var (results, totalCount) = await _service.SearchCommercialToolsWithCount(queryUrl);

        var resultList = results.ToList();
        resultList.Should().HaveCount(1);
        totalCount.Should().Be(1);

        var searchResult = resultList.First();
        searchResult.Id.Should().Be("ocds-h6vhtk-04f907");
        searchResult.Title.Should().Be("IT Services Framework");
        searchResult.Description.Should().Be("Test procurement description");
        searchResult.PublishedDate.Should().Be(new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc));
        searchResult.SubmissionDeadline.Should().Be(new DateTime(2025, 3, 15, 23, 59, 59, DateTimeKind.Utc));
        searchResult.Fees.Should().Be(0);
        searchResult.AwardMethod.Should().Be("With competition");
        searchResult.Status.Should().Be(CommercialToolStatus.Active);
        searchResult.OtherContractingAuthorityCanUse.Should().Be("Yes");
    }

    [Fact]
    public async Task SearchCommercialToolsWithCount_WhenNullApiResponse_ShouldReturnEmptyResults()
    {
        var queryUrl = "https://api.example.com/tenders?filter=test";
        var jsonResponse = "null";

        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var (results, totalCount) = await _service.SearchCommercialToolsWithCount(queryUrl);

        results.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchCommercialTools_WhenHttpRequestFails_ShouldThrow()
    {
        var queryUrl = "https://api.example.com/tenders?filter=test";
        SetupHttpResponse(HttpStatusCode.InternalServerError, "Server Error");

        var action = async () => await _service.SearchCommercialToolsWithCount(queryUrl);
        await action.Should().ThrowAsync<HttpRequestException>();
    }


    [Fact]
    public async Task SearchCommercialTools_WhenValidResponse_ShouldCallMapperWithCorrectData()
    {
        var queryUrl = "https://api.example.com/tenders?filter=test";
        var jsonResponse = """
        {
            "success": true,
            "statusCode": 200,
            "message": "Success",
            "data": [
                {
                    "id": "test-id",
                    "ocid": "ocds-test",
                    "buyer": {
                        "name": "Test Buyer"
                    },
                    "tender": {
                        "tenderId": "test-tender-id",
                        "tenderIdentifier": "ocds-test",
                        "title": "Test Tender"
                    }
                }
            ],
            "paging": {
                "totalItems": 5
            }
        }
        """;

        var expectedResult = new SearchResultDto { Id = "test-id", Title = "Test Tender" };
        _mockMapper.Setup(m => m.Map<SearchResultDto>(It.IsAny<CommercialToolApiItem>()))
            .Returns(expectedResult);

        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var (results, totalCount) = await _service.SearchCommercialToolsWithCount(queryUrl);

        var resultList = results.ToList();
        resultList.Should().HaveCount(1);
        totalCount.Should().Be(5);
        resultList.First().Should().Be(expectedResult);

        _mockMapper.Verify(m => m.Map<SearchResultDto>(It.Is<CommercialToolApiItem>(
            item => item.Id == "test-id" &&
                    item.Ocid == "ocds-test" &&
                    item.Tender != null && item.Tender.Title == "Test Tender")), Times.Once);
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
}