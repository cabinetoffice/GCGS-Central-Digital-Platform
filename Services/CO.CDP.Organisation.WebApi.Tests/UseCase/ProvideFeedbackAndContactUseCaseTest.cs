using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class ProvideFeedbackAndContactUseCaseTest
{
    private readonly Mock<IGovUKNotifyApiClient> _mockGovUKNotifyApiClient;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly ProvideFeedbackAndContactUseCase _useCase;
    private readonly Mock<ILogger<ProvideFeedbackAndContactUseCase>> _logger=new();

    public ProvideFeedbackAndContactUseCaseTest()
    {
        _mockGovUKNotifyApiClient = new Mock<IGovUKNotifyApiClient>();
        _mockConfiguration = new Mock<IConfiguration>();        
        _useCase = new ProvideFeedbackAndContactUseCase(_mockGovUKNotifyApiClient.Object, _mockConfiguration.Object, _logger.Object);
    }

    [Fact]
    public async Task Execute_ShouldReturnTrue_WhenEmailIsSentSuccessfully()
    {
        var provideFeedback = new ProvideFeedbackAndContact
        {
            Subject = "Support",
            FeedbackAbout = "Test Feedback About",
            SpecificPage = "Test Specific Page",
            Feedback = "Test Feedback",
            Name = "Test Name",
            Email = "test@example.com"
        };

        var expectedResponse = new EmailNotificationResponse
        {
            Content = It.IsAny<EmailResponseContent>(),
            Id = Guid.NewGuid(),
            Template = It.IsAny<Template>(),
            Uri = It.IsAny<Uri>()
        };

        _mockGovUKNotifyApiClient.Setup(c => c.SendEmail(It.IsAny<EmailNotificationRequest>()))
            .ReturnsAsync(expectedResponse);

        var result = await _useCase.Execute(provideFeedback);

        result.Should().BeTrue();

        _mockGovUKNotifyApiClient.Verify(c => c.SendEmail(It.IsAny<EmailNotificationRequest>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldReturnFalse_WhenEmailSendingFails()
    {
        var provideFeedback = new ProvideFeedbackAndContact
        {
            Subject = "Feeback",
            FeedbackAbout = "Test Feedback About",
            SpecificPage = "Test Specific Page",
            Feedback = "Test Feedback",
            Name = "Test Name",
            Email = "test@example.com"
        };

        _mockGovUKNotifyApiClient.Setup(c => c.SendEmail(It.IsAny<EmailNotificationRequest>()))
            .ThrowsAsync(new Exception("Email sending failed"));

        var result = await _useCase.Execute(provideFeedback);

        result.Should().BeFalse();

        _mockGovUKNotifyApiClient.Verify(c => c.SendEmail(It.IsAny<EmailNotificationRequest>()), Times.Once);
    }
}