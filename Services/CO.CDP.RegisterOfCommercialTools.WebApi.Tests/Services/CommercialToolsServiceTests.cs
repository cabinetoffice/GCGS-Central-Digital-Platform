using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.RegisterOfCommercialTools.WebApi.Services;
using FluentAssertions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Tests.Services;

public class CommercialToolsServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
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
        _service = new CommercialToolsService(httpClient, mockConfiguration.Object);
    }

    [Fact]
    public async Task SearchCommercialTools_ShouldReturnMappedResults()
    {
        var queryUrl = "https://api.example.com/tenders?filter=test";
        var jsonResponse = """
        {
            "data": [
                {
                    "name": "IT Services Framework",
                    "status": "active",
                    "additionalProperties": {
                        "procurementId": "003033-2025",
                        "tenderId": "tender-123",
                        "uri": "https://example.com/tender/1",
                        "procuringEntity": "Test Authority",
                        "effectiveEndDateUtc": "2025-03-15T23:59:59Z"
                    },
                    "documents": [
                        {
                            "datePublished": {
                                "dateTime": {
                                    "value": "2025-01-15T10:00:00Z"
                                }
                            }
                        }
                    ],
                    "participationFees": [
                        {
                            "relativeValue": {
                                "proportion": 0.05
                            }
                        }
                    ],
                    "tender": {
                        "techniques": {
                            "hasFrameworkAgreement": true,
                            "frameworkAgreement": {
                                "method": "open"
                            }
                        }
                    }
                }
            ]
        }
        """;

        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var result = await _service.SearchCommercialTools(queryUrl);

        var resultList = result.ToList();
        resultList.Should().HaveCount(1);

        var searchResult = resultList.First();
        searchResult.Id.Should().Be("003033-2025");
        searchResult.Title.Should().Be("IT Services Framework");
        searchResult.Description.Should().Be("Test Authority");
        searchResult.PublishedDate.Should().Be(new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc));
        searchResult.SubmissionDeadline.Should().Be(new DateTime(2025, 3, 15, 23, 59, 59, DateTimeKind.Utc));
        searchResult.Fees.Should().Be(5.0m);
        searchResult.AwardMethod.Should().Be("Unknown");
        searchResult.Status.Should().Be(CommercialToolStatus.Active);
    }

    [Fact]
    public async Task SearchCommercialTools_WhenReleasesEmpty_ShouldUseFallbackId()
    {
        var queryUrl = "https://api.example.com/tenders?filter=test";
        var jsonResponse = """
        {
            "data": [
                {
                    "name": "Test Framework",
                    "status": "active",
                    "additionalProperties": {
                        "procurementId": "test-procurement-id"
                    }
                }
            ]
        }
        """;

        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var result = await _service.SearchCommercialTools(queryUrl);

        var resultList = result.ToList();
        resultList.Should().HaveCount(1);
        resultList.First().Id.Should().Be("test-procurement-id");
    }

    [Fact]
    public async Task SearchCommercialTools_ShouldMapStatusCorrectly()
    {
        var queryUrl = "https://api.example.com/tenders?filter=test";
        var jsonResponse = """
        {
            "data": [
                {
                    "name": "Test",
                    "status": "cancelled",
                    "additionalProperties": {
                        "procurementId": "test-123"
                    }
                },
                {
                    "name": "Test2",
                    "status": "active",
                    "additionalProperties": {
                        "procurementId": "test-456",
                        "effectiveEndDateUtc": "2026-01-15T23:59:59Z"
                    }
                }
            ]
        }
        """;

        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var result = await _service.SearchCommercialTools(queryUrl);

        var resultList = result.ToList();
        resultList.Should().HaveCount(2);

        resultList[0].Status.Should().Be(CommercialToolStatus.Closed);

        resultList[1].Status.Should().Be(CommercialToolStatus.Active);
    }

    [Fact]
    public async Task SearchCommercialTools_WhenStatusIsUnknown_ShouldSetUnknownStatusToActive()
    {
        var queryUrl = "https://api.example.com/tenders?filter=test";
        var jsonResponse = """
        {
            "data": [
                {
                    "name": "Test",
                    "status": "InvalidStatus",
                    "additionalProperties": {
                        "procurementId": "test-123"
                    }
                }
            ]
        }
        """;

        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var result = await _service.SearchCommercialTools(queryUrl);

        var resultList = result.ToList();
        resultList.Should().HaveCount(1);
        resultList.First().Status.Should().Be(CommercialToolStatus.Active);
    }

    [Fact]
    public async Task SearchCommercialTools_WhenHttpRequestFails_ShouldThrow()
    {
        var queryUrl = "https://api.example.com/tenders?filter=test";
        SetupHttpResponse(HttpStatusCode.InternalServerError, "Server Error");

        var action = async () => await _service.SearchCommercialTools(queryUrl);
        await action.Should().ThrowAsync<HttpRequestException>();
    }


    [Fact]
    public async Task SearchCommercialTools_WhenEmptyResponse_ShouldReturnEmptyList()
    {
        var queryUrl = "https://api.example.com/tenders?filter=test";
        SetupHttpResponse(HttpStatusCode.OK, "{\"data\": []}");

        var result = await _service.SearchCommercialTools(queryUrl);

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
}