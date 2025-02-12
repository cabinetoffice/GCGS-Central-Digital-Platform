using CO.CDP.GovUKNotify.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CO.CDP.GovUKNotify.Test;

public class GovUKNotifyApiClientTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<IAuthentication> _mockAuthentication;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly GovUKNotifyApiClient _govUKNotifyApiClient;
    private readonly Mock<ILogger<GovUKNotifyApiClient>> _logger;
    private readonly Mock<IConfiguration> _configuration;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
    public GovUKNotifyApiClientTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockAuthentication = new Mock<IAuthentication>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _logger = new Mock<ILogger<GovUKNotifyApiClient>>();
        _httpContextAccessor = new Mock<IHttpContextAccessor>();
        _configuration = new Mock<IConfiguration>();

        var client = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.notifications.service.gov.uk")
        };

        _mockAuthentication.Setup(a => a.GetAuthenticationHeader())
            .Returns(new AuthenticationHeaderValue("Bearer", "dummy_token"));

        _mockHttpClientFactory.Setup(h => h.CreateClient(It.IsAny<string>()))
            .Returns(client);

        _mockHttpClientFactory.Setup(h => h.CreateClient(It.IsAny<string>()))
        .Returns(client);

        var mockHttpContext = new DefaultHttpContext();
        _httpContextAccessor.Setup(_ => _.HttpContext).Returns(mockHttpContext);

        _govUKNotifyApiClient = new GovUKNotifyApiClient(_mockHttpClientFactory.Object, _mockAuthentication.Object, _logger.Object, _configuration.Object, _httpContextAccessor.Object);
    }

    [Fact]
    public async Task SendEmail_ValidRequest_ReturnsSuccessResponse()
    {
        var templateId = Guid.NewGuid();
        var responseId = Guid.NewGuid();

        var emailNotificationRequest = new EmailNotificationRequest
        {
            EmailAddress = "test.test.com",
            TemplateId = templateId.ToString()
        };

        var expectedResponse = new EmailNotificationResponse
        {
            Content = It.IsAny<EmailResponseContent>(),
            Id = responseId,
            Template = It.IsAny<Template>(),
            Uri = It.IsAny<Uri>()
        };

        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedResponse)
        };
        
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        var response = await _govUKNotifyApiClient.SendEmail(emailNotificationRequest);

        response.Should().NotBeNull();
        response.As<EmailNotificationResponse>().Id.Should().Be(expectedResponse.Id);
    }

    [Fact]
    public async Task SendEmail_HttpRequestException_ThrowsException()
    {
        var emailNotificationRequest = new EmailNotificationRequest
        {
            EmailAddress = "test@test.com",
            TemplateId = Guid.NewGuid().ToString()
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Error occurred"));

        Func<Task> act = async () => await _govUKNotifyApiClient.SendEmail(emailNotificationRequest);
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task SendEmail_ResponseNotSuccessful_ThrowsHttpRequestException()
    {        
        var emailNotificationRequest = new EmailNotificationRequest
        {
            EmailAddress = "test@test.com",
            TemplateId = Guid.NewGuid().ToString()
        };

        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponseMessage);
        Func<Task> act = async () => await _govUKNotifyApiClient.SendEmail(emailNotificationRequest);
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task SendEmail_WithBypassEnabled_DoesNotSend()
    {
        var templateId = Guid.NewGuid();
        var responseId = Guid.NewGuid();

        var emailNotificationRequest = new EmailNotificationRequest
        {
            EmailAddress = "test.test.com",
            TemplateId = templateId.ToString()
        };

        var expectedResponse = new EmailNotificationResponse
        {
            Content = It.IsAny<EmailResponseContent>(),
            Id = responseId,
            Template = It.IsAny<Template>(),
            Uri = It.IsAny<Uri>()
        };

        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedResponse)
        };

        var configurationWithNotifyHeaderEnabled = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new("Features:EnableNotifyHeaderBypass", "true"),
            ])
            .Build();

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockHttpContext = new DefaultHttpContext();
        mockHttpContext.Request.Headers["BypassNotify"] = "true";
        _httpContextAccessor.Setup(_ => _.HttpContext).Returns(mockHttpContext);

        var govUKNotifyApiClient = new GovUKNotifyApiClient(_mockHttpClientFactory.Object, _mockAuthentication.Object, _logger.Object, configurationWithNotifyHeaderEnabled, _httpContextAccessor.Object);

        var response = await govUKNotifyApiClient.SendEmail(emailNotificationRequest);

        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );

        response.Should().BeNull();
    }
}