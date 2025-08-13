using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using FluentAssertions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;

namespace CO.CDP.RegisterOfCommercialTools.WebApiClient.Tests;

public class AuthenticationIntegrationTests
{
    [Fact]
    public async Task SearchAsync_PreservesAuthenticationHeaders_WhenMakingApiCall()
    {
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        HttpRequestMessage? capturedRequest = null;

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
            {
                capturedRequest = request;
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

        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");

        var client = new CommercialToolsApiClient(httpClient);
        var request = new SearchRequestDto { Keyword = "test" };

        await client.SearchAsync(request);

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Authorization.Should().NotBeNull();
        capturedRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        capturedRequest.Headers.Authorization.Parameter.Should().Be("test-token");
    }

    [Fact]
    public async Task SearchAsync_WorksWithCustomHttpMessageHandlers()
    {
        var customHandler = new TestAuthHandler();
        var mockInnerHandler = new Mock<HttpMessageHandler>();

        mockInnerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new SearchResponse { TotalCount = 5 })
            });

        customHandler.InnerHandler = mockInnerHandler.Object;
        var httpClient = new HttpClient(customHandler)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };

        var client = new CommercialToolsApiClient(httpClient);
        var request = new SearchRequestDto { Keyword = "test" };

        var result = await client.SearchAsync(request);

        result.Should().NotBeNull();
        result!.TotalCount.Should().Be(5);
        customHandler.RequestProcessed.Should().BeTrue();
    }

    private class TestAuthHandler : DelegatingHandler
    {
        public bool RequestProcessed { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "injected-token");

            RequestProcessed = true;

            return await base.SendAsync(request, cancellationToken);
        }
    }
}