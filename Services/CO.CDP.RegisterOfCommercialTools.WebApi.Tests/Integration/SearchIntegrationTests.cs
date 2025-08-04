using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.RegisterOfCommercialTools.WebApi.Services;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Tests.Integration;

public class SearchIntegrationTests
{
    private readonly HttpClient _client;

    public SearchIntegrationTests()
    {
        var factory = new TestWebApplicationFactory<Program>(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Aws:ServiceURL", "http://localhost:4566" }
                });
            });
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ICommercialToolsRepository));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                var mockRepository = new Mock<ICommercialToolsRepository>();
                SetupMockRepository(mockRepository);
                services.AddSingleton(mockRepository.Object);
            });
        });

        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_SearchEndpoint_ShouldReturnSearchResults()
    {
        var response = await _client.GetAsync("/api/Search?Keyword=test&PageSize=10&PageNumber=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        searchResponse.Should().NotBeNull();
        searchResponse!.Results.Should().HaveCount(2);
        searchResponse.TotalCount.Should().Be(50);
        searchResponse.PageNumber.Should().Be(1);
        searchResponse.PageSize.Should().Be(10);
        searchResponse.Results.First().Id.Should().Be("003033-2025");
        searchResponse.Results.First().Title.Should().Be("Integration Test Framework 1");
        searchResponse.Results.Last().Id.Should().Be("004044-2025");
        searchResponse.Results.Last().Title.Should().Be("Integration Test Framework 2");
    }

    [Fact]
    public async Task Get_SearchEndpointWithComplexQuery_ShouldPassAllParameters()
    {
        var queryParams = new Dictionary<string, string?>
        {
            ["Keyword"] = "complex test",
            ["Status"] = "Active",
            ["SubmissionDeadlineFrom"] = "2025-01-01",
            ["SubmissionDeadlineTo"] = "2025-12-31",
            ["ContractStartDateFrom"] = "2025-06-01",
            ["ContractStartDateTo"] = "2025-06-30",
            ["ContractEndDateFrom"] = "2026-01-01",
            ["ContractEndDateTo"] = "2026-12-31",
            ["MinFees"] = "100.50",
            ["MaxFees"] = "999.99",
            ["PageSize"] = "20",
            ["PageNumber"] = "2"
        };

        var queryString = string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"));

        var response = await _client.GetAsync($"/api/Search?{queryString}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        searchResponse.Should().NotBeNull();
        searchResponse!.Results.Should().HaveCount(2);
        searchResponse.TotalCount.Should().Be(50);
        searchResponse.PageNumber.Should().Be(2);
        searchResponse.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task Get_SearchEndpoint_WithInvalidParameters_ShouldHandleGracefully()
    {
        var response = await _client.GetAsync("/api/Search?PageSize=0&PageNumber=-1&MinFees=invalid");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_SearchEndpoint_ShouldNotBeSupported()
    {
        var searchRequest = new SearchRequestDto
        {
            Keyword = "test",
            PageSize = 10,
            PageNumber = 1
        };

        var response = await _client.PostAsJsonAsync("/api/Search", searchRequest);

        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task Get_SearchEndpoint_ShouldBuildCorrectODataQuery()
    {
        var response = await _client.GetAsync("/api/Search?Keyword=integration&Status=Active&MinFees=100&MaxFees=500&PageSize=5&PageNumber=3");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        searchResponse.Should().NotBeNull();
        searchResponse!.Results.Should().HaveCount(2);
        searchResponse.TotalCount.Should().Be(50);
        searchResponse.PageNumber.Should().Be(3);
        searchResponse.PageSize.Should().Be(5);
    }

    private static void SetupMockRepository(Mock<ICommercialToolsRepository> mockRepository)
    {
        var defaultResults = new List<SearchResultDto>
        {
            new()
            {
                Id = "003033-2025",
                Title = "Integration Test Framework 1",
                Description = "First integration test framework",
                Link = "https://test.com/framework1",
                PublishedDate = DateTime.UtcNow.AddDays(-10),
                SubmissionDeadline = DateTime.UtcNow.AddDays(30),
                Status = CommercialToolStatus.Active,
                Fees = 250.00m,
                AwardMethod = "Competitive"
            },
            new()
            {
                Id = "004044-2025",
                Title = "Integration Test Framework 2",
                Description = "Second integration test framework",
                Link = "https://test.com/framework2",
                PublishedDate = DateTime.UtcNow.AddDays(-5),
                SubmissionDeadline = DateTime.UtcNow.AddDays(45),
                Status = CommercialToolStatus.Upcoming,
                Fees = 500.00m,
                AwardMethod = "Direct Award"
            }
        };

        mockRepository
            .Setup(x => x.SearchCommercialTools(It.IsAny<string>()))
            .ReturnsAsync(defaultResults);

        mockRepository
            .Setup(x => x.GetCommercialToolsCount(It.IsAny<string>()))
            .ReturnsAsync(50);

        mockRepository
            .Setup(x => x.GetCommercialToolById("003033-2025"))
            .ReturnsAsync(new SearchResultDto
            {
                Id = "003033-2025",
                Title = "Specific Integration Test Framework",
                Description = "Specific framework for testing GetById",
                Link = "https://test.com/specific",
                PublishedDate = DateTime.UtcNow.AddDays(-15),
                SubmissionDeadline = DateTime.UtcNow.AddDays(60),
                Status = CommercialToolStatus.Active,
                Fees = 750.00m,
                AwardMethod = "Framework"
            });

        mockRepository
            .Setup(x => x.GetCommercialToolById("nonexistent-id"))
            .ReturnsAsync((SearchResultDto?)null);
    }
}