using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Xml.Linq;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class ProvideFeedbackAndContactUseCaseTest
{
    private readonly Mock<IGovUKNotifyApiClient> _mockGovUKNotifyApiClient;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<ProvideFeedbackAndContactUseCase>> _mockLogger;
    private readonly ProvideFeedbackAndContactUseCase _useCase;

    public ProvideFeedbackAndContactUseCaseTest()
    {
        _mockGovUKNotifyApiClient = new Mock<IGovUKNotifyApiClient>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<ProvideFeedbackAndContactUseCase>>();

        _useCase = new ProvideFeedbackAndContactUseCase(
            _mockGovUKNotifyApiClient.Object,
            _mockConfiguration.Object,
            _mockLogger.Object
        );
    } 

    [Fact]
    public async Task Execute_ShouldReturnFalse_WhenConfigurationKeysAreMissing()
    {        
        _mockConfiguration.Setup(c => c["GOVUKNotify:ProvideFeedbackAndContactEmailTemplateId"]).Returns((string)null);
        _mockConfiguration.Setup(c => c["GOVUKNotify:SupportAdminEmailAddress"]).Returns((string)null);
        var command = new ProvideFeedbackAndContact()
        {
            FeedbackAbout = "Website",
            SpecificPage = "Homepage",
            Feedback = "Great site!",
            Name = "John Doe",
            Email = "john.doe@test.com",
            Subject = "Feedback"
        };
       
        var result = await _useCase.Execute(command);
       
        result.Should().BeFalse();       
        _mockGovUKNotifyApiClient.Verify(c => c.SendEmail(It.IsAny<EmailNotificationRequest>()), Times.Never);
    }

    [Fact]
    public async Task Execute_ShouldReturnTrue_WhenEmailIsSentSuccessfully()
    {        
        _mockConfiguration.Setup(c => c["GOVUKNotify:ProvideFeedbackAndContactEmailTemplateId"]).Returns("template-id");
        _mockConfiguration.Setup(c => c["GOVUKNotify:SupportAdminEmailAddress"]).Returns("admin@test.com");
        var command = new ProvideFeedbackAndContact
        {
            FeedbackAbout = "Website",
            SpecificPage = "Homepage",
            Feedback = "Great site!",
            Name = "John Doe",
            Email = "john.doe@test.com",
            Subject = "Feedback"
        };
       
        var result = await _useCase.Execute(command);

        result.Should().BeTrue();
        _mockGovUKNotifyApiClient.Verify(c => c.SendEmail(It.IsAny<EmailNotificationRequest>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldReturnFalse_WhenEmailSendFails()
    {       
        _mockConfiguration.Setup(c => c["GOVUKNotify:ProvideFeedbackAndContactEmailTemplateId"]).Returns("template-id");
        _mockConfiguration.Setup(c => c["GOVUKNotify:SupportAdminEmailAddress"]).Returns("admin@test.com");
        var command = new ProvideFeedbackAndContact
        {
            FeedbackAbout = "Website",
            SpecificPage = "Homepage",
            Feedback = "Great site!",
            Name = "John Doe",
            Email = "john.doe@test.com",
            Subject = "Feedback"
        };

        _mockGovUKNotifyApiClient
            .Setup(c => c.SendEmail(It.IsAny<EmailNotificationRequest>()))
            .ThrowsAsync(new Exception());
       
        var result = await _useCase.Execute(command);

         result.Should().BeFalse();
        _mockGovUKNotifyApiClient.Verify(c => c.SendEmail(It.IsAny<EmailNotificationRequest>()), Times.Once);
    }
}