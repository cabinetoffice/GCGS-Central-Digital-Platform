using CO.CDP.Functional;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.WebApi.Foundation;
using FluentAssertions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;

namespace CO.CDP.RegisterOfCommercialTools.WebApiClient.Tests;

public class CommercialToolsApiClientTests
{
    [Fact]
    public async Task SearchAsync_ReturnsSearchResponse_WhenApiCallSucceeds()
    {
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var expectedResponse = new SearchResponse
        {
            Results = new[]
            {
                new SearchResultDto
                {
                    Id = "1",
                    Title = "Test Tool",
                    BuyerName = "Test Description",
                    Status = CommercialToolStatus.Active,
                    MaximumFee = "10%",
                    AwardMethod = "Open"
                }
            },
            TotalCount = 1,
            PageNumber = 1,
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedResponse)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };

        var client = new CommercialToolsApiClient(httpClient);
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            PageNumber = 1,
        };

        var result = await client.SearchAsync(request);

        result.IsRight().Should().BeTrue();
        result.Match(
            error => Assert.Fail($"Expected success but got error: {error.Message}"),
            success =>
            {
                success.TotalCount.Should().Be(1);
                success.Results.Should().HaveCount(1);
                success.Results.First().Title.Should().Be("Test Tool");
            });
    }

    [Fact]
    public async Task SearchAsync_ReturnsClientError_WhenApiCallFails()
    {
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };

        var client = new CommercialToolsApiClient(httpClient);
        var request = new SearchRequestDto { Keywords = ["test"] };

        var result = await client.SearchAsync(request);

        result.IsLeft().Should().BeTrue();
        result.Match(
            error =>
            {
                error.Should().BeOfType<ClientError>();
                var clientError = (ClientError)error;
                clientError.StatusCode.Should().Be(HttpStatusCode.NotFound);
            },
            _ => Assert.Fail("Expected error but got success"));
    }

    [Theory]
    [InlineData("test keyword", "test%20keyword")]
    [InlineData("special&chars", "special%26chars")]
    [InlineData("query+params", "query%2Bparams")]
    public async Task SearchAsync_ProperlyEncodesQueryParameters(string keyword, string expectedEncoded)
    {
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        string? actualQuery = null;

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
            {
                actualQuery = request.RequestUri?.Query;
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new SearchResponse())
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };

        var client = new CommercialToolsApiClient(httpClient);
        var request = new SearchRequestDto { Keywords = [keyword] };

        await client.SearchAsync(request);

        actualQuery.Should().NotBeNullOrEmpty();
        actualQuery.Should().Contain($"Keywords={expectedEncoded}");
    }

    [Fact]
    public async Task SearchAsync_IncludesAllNonNullProperties_InQueryString()
    {
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        string? actualQuery = null;

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
            {
                actualQuery = request.RequestUri?.Query;
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new SearchResponse())
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };

        var client = new CommercialToolsApiClient(httpClient);
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            SearchMode = KeywordSearchMode.Any,
            Status = "Active",
            SortBy = "Title",
            PageNumber = 2,
            MinFees = 100m,
            MaxFees = 1000m
        };

        await client.SearchAsync(request);

        actualQuery.Should().Contain("Keywords=test");
        actualQuery.Should().Contain("SearchMode=Any");
        actualQuery.Should().Contain("Status=Active");
        actualQuery.Should().Contain("SortBy=Title");
        actualQuery.Should().Contain("PageNumber=2");
        actualQuery.Should().Contain("MinFees=100");
        actualQuery.Should().Contain("MaxFees=1000");
    }

    [Fact]
    public async Task SearchAsync_ExcludesNullProperties_FromQueryString()
    {
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        string? actualQuery = null;

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
            {
                actualQuery = request.RequestUri?.Query;
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new SearchResponse())
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };

        var client = new CommercialToolsApiClient(httpClient);
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            SearchMode = KeywordSearchMode.Any,
            Status = null,
            SortBy = null,
            MinFees = null,
            MaxFees = null
        };

        await client.SearchAsync(request);

        actualQuery.Should().Contain("Keywords=test");
        actualQuery.Should().Contain("SearchMode=Any");
        actualQuery.Should().NotContain("Status=");
        actualQuery.Should().NotContain("SortBy=");
        actualQuery.Should().NotContain("MinFees=");
        actualQuery.Should().NotContain("MaxFees=");
    }

    [Fact]
    public async Task SearchAsync_HandlesDateTimeProperties_Correctly()
    {
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        string? actualQuery = null;

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
            {
                actualQuery = request.RequestUri?.Query;
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new SearchResponse())
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };

        var client = new CommercialToolsApiClient(httpClient);
        var testDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var request = new SearchRequestDto
        {
            Keywords = ["test"],
            SubmissionDeadlineFrom = testDate,
            SubmissionDeadlineTo = testDate.AddDays(7)
        };

        await client.SearchAsync(request);

        actualQuery.Should().Contain("SubmissionDeadlineFrom=2024-01-15");
        actualQuery.Should().Contain("SubmissionDeadlineTo=2024-01-22");
    }
}