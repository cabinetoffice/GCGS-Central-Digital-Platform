using GovukNotify.Models;
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

    public GovUKNotifyApiClientTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockAuthentication = new Mock<IAuthentication>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        var client = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.notifications.service.gov.uk")
        };

        _mockAuthentication.Setup(a => a.GetAuthenticationHeader())
            .Returns(new AuthenticationHeaderValue("Bearer", "dummy_token"));

        _mockHttpClientFactory.Setup(h => h.CreateClient(It.IsAny<string>()))
            .Returns(client);

        _govUKNotifyApiClient = new GovUKNotifyApiClient(_mockHttpClientFactory.Object, _mockAuthentication.Object);
    }

    [Fact]
    public async Task SendEmail_ValidRequest_ReturnsSuccessResponse()
    {
        var templateId = Guid.NewGuid();
        var responseId = Guid.NewGuid();

        var emailNotificationRequest = new EmailNotificationResquest
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

        Assert.NotNull(response);
        Assert.Equal(expectedResponse.Id, response.Id);
    }

    [Fact]
    public async Task SendEmail_HttpRequestException_ThrowsException()
    {
        var emailNotificationRequest = new EmailNotificationResquest
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

        await Assert.ThrowsAsync<HttpRequestException>(() => _govUKNotifyApiClient.SendEmail(emailNotificationRequest));
    }

    [Fact]
    public async Task SendEmail_ResponseNotSuccessful_ThrowsHttpRequestException()
    {        
        var emailNotificationRequest = new EmailNotificationResquest
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
        
        await Assert.ThrowsAsync<HttpRequestException>(() => _govUKNotifyApiClient.SendEmail(emailNotificationRequest));
    }
}