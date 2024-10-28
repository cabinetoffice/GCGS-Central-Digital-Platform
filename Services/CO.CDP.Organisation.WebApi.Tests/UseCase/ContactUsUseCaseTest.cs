using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class ContactUsUseCaseTest
{
    private readonly Mock<IGovUKNotifyApiClient> _mockGovUKNotifyApiClient = new();
    private readonly Mock<IConfiguration> _mockConfiguration = new();
    private readonly Mock<ILogger<ContactUsUseCase>> _mockLogger = new();
    private readonly ContactUsUseCase _useCase;

    public ContactUsUseCaseTest()
    {
        _mockConfiguration.Setup(c => c["GOVUKNotify:ContactEmailTemplateId"]).Returns("templateId");
        _mockConfiguration.Setup(c => c["GOVUKNotify:SupportAdminEmailAddress"]).Returns("admin@example.com");

        _useCase = new ContactUsUseCase(
            _mockGovUKNotifyApiClient.Object,
            _mockConfiguration.Object,
            _mockLogger.Object
        );
    }
    [Fact]
    public async Task Execute_ShouldReturnTrue_WhenEmailIsSentSuccessfully()
    {
        var command = new ContactUs
        {
            EmailAddress = "test@example.com",
            Message = "Message",
            Name = "User",
            OrganisationName = "Organisation"
        };

        _mockGovUKNotifyApiClient.Setup(api => api.SendEmail(It.IsAny<EmailNotificationRequest>()));

        var result = await _useCase.Execute(command);

        result.Should().BeTrue();
        _mockGovUKNotifyApiClient.Verify(api => api.SendEmail(It.IsAny<EmailNotificationRequest>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldReturnFalse_WhenEmailSendFails()
    {
        var command = new ContactUs
        {
            EmailAddress = "test@example.com",
            Message = "Message",
            Name = "User",
            OrganisationName = "Organisation"
        };

        _mockGovUKNotifyApiClient.Setup(api => api.SendEmail(It.IsAny<EmailNotificationRequest>()))
            .ThrowsAsync(new Exception("Email send failure"));

        var result = await _useCase.Execute(command);

        result.Should().BeFalse();
        _mockGovUKNotifyApiClient.Verify(api => api.SendEmail(It.IsAny<EmailNotificationRequest>()), Times.Once);
    }
}