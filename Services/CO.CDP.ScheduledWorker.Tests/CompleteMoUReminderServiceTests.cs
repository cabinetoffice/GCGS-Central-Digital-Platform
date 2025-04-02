using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.ScheduledWorker.Tests;

public class CompleteMoUReminderServiceTests
{
    private readonly Mock<ILogger<CompleteMoUReminderService>> _mockLogger;
    private readonly Mock<IMouRepository> _mockMouRepository;
    private readonly Mock<IGovUKNotifyApiClient> _mockGovUKNotifyClient;
    private readonly CompleteMoUReminderService _service;

    public CompleteMoUReminderServiceTests()
    {
        _mockLogger = new Mock<ILogger<CompleteMoUReminderService>>();
        _mockMouRepository = new Mock<IMouRepository>();
        _mockGovUKNotifyClient = new Mock<IGovUKNotifyApiClient>();

        var inMemorySettings = new List<KeyValuePair<string, string?>>
        {
            new("GOVUKNotify:MouReminderToSignEmailTemplateId", "test-template-id"),
            new("OrganisationAppUrl", "http://baseurl/"),
        };

        _service = new CompleteMoUReminderService(
            _mockLogger.Object,
            _mockMouRepository.Object,
            new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build(),
            _mockGovUKNotifyClient.Object);
    }

    [Fact]
    public async Task ExecuteWorkAsync_ShouldProcessReminders_WhenRemindersExist()
    {
        List<MouReminderOrganisation> organisations =
            [new() { Id = 1, Name = "Org1", Email = "test1@example.com,test2@example.com", Guid = Guid.NewGuid() }];

        _mockMouRepository.Setup(r => r.GetMouReminderOrganisations()).ReturnsAsync(organisations);
        _mockMouRepository.Setup(r => r.UpsertMouEmailReminder(1)).Returns(Task.CompletedTask);

        await _service.ExecuteWorkAsync(CancellationToken.None);

        _mockMouRepository.Verify(r => r.GetMouReminderOrganisations(), Times.Once);
        _mockMouRepository.Verify(r => r.UpsertMouEmailReminder(1), Times.Once);
        _mockGovUKNotifyClient.Verify(n => n.SendEmail(It.IsAny<EmailNotificationRequest>()), Times.Exactly(2));

        _mockLogger.Verify(
            l => l.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Sent MoU Reminder Email For Org1."),
                It.IsAny<Exception?>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)), Times.Once);
    }

    [Fact]
    public async Task ExecuteWorkAsync_ShouldNotCallNotification_WhenNoRemindersExist()
    {
        _mockMouRepository.Setup(r => r.GetMouReminderOrganisations())
            .ReturnsAsync([]);

        await _service.ExecuteWorkAsync(CancellationToken.None);

        _mockMouRepository.Verify(r => r.UpsertMouEmailReminder(It.IsAny<int>()), Times.Never);
        _mockGovUKNotifyClient.Verify(n => n.SendEmail(It.IsAny<EmailNotificationRequest>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteWorkAsync_ShouldLogError_WhenEmailFails()
    {
        List<MouReminderOrganisation> organisations =
            [new() { Id = 1, Name = "Test Org", Email = "test1@example.com,test2@example.com", Guid = Guid.NewGuid() }];

        _mockMouRepository.Setup(r => r.GetMouReminderOrganisations())
            .ReturnsAsync(organisations);

        _mockGovUKNotifyClient.Setup(n => n.SendEmail(It.IsAny<EmailNotificationRequest>()))
            .ThrowsAsync(new Exception("Email error"));

        await _service.ExecuteWorkAsync(CancellationToken.None);

        _mockLogger.Verify(
            l => l.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Failed to send Mou reminder email for: Test Org"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)), Times.Once);
    }

    [Fact]
    public async Task ExecuteWorkAsync_ShouldSendCorrectEmail_WithValidLinkFormat()
    {
        List<MouReminderOrganisation> organisations =
            [new() { Id = 1, Name = "Org1", Email = "test1@example.com,test2@example.com", Guid = Guid.NewGuid() }];

        _mockMouRepository.Setup(r => r.GetMouReminderOrganisations()).ReturnsAsync(organisations);
        _mockMouRepository.Setup(r => r.UpsertMouEmailReminder(1)).Returns(Task.CompletedTask);

        EmailNotificationRequest? capturedRequest = null;

        _mockGovUKNotifyClient
            .Setup(client => client.SendEmail(It.IsAny<EmailNotificationRequest>()))
            .Callback<EmailNotificationRequest>(req => capturedRequest = req);

        await _service.ExecuteWorkAsync(CancellationToken.None);

        capturedRequest.Should().NotBeNull();
        capturedRequest!.EmailAddress.Should().Be("test2@example.com");
        capturedRequest.TemplateId.Should().Be("test-template-id");
        capturedRequest.Personalisation.Should().ContainKey("org_name").WhoseValue.Should().Be("Org1");
        capturedRequest.Personalisation.Should().ContainKey("link").WhoseValue.Should()
                .Be($"http://baseurl/organisation/{organisations[0].Guid}/review-and-sign-memorandom");
    }

    [Fact]
    public async Task ExecuteWorkAsync_ShouldThrowError_WhenOrganisationAppUrlConfigNotSet()
    {
        List<MouReminderOrganisation> organisations =
            [new() { Id = 1, Name = "Org1", Email = "test1@example.com,test2@example.com", Guid = Guid.NewGuid() }];

        _mockMouRepository.Setup(r => r.GetMouReminderOrganisations()).ReturnsAsync(organisations);
        _mockMouRepository.Setup(r => r.UpsertMouEmailReminder(1)).Returns(Task.CompletedTask);

        var service = new CompleteMoUReminderService(
            _mockLogger.Object,
            _mockMouRepository.Object,
            new ConfigurationBuilder().AddInMemoryCollection([new("GOVUKNotify:MouReminderToSignEmailTemplateId", "test-template-id")]).Build(),
            _mockGovUKNotifyClient.Object);

        Func<Task> act = async () => await service.ExecuteWorkAsync(CancellationToken.None);

        await act.Should()
           .ThrowAsync<Exception>()
           .WithMessage("Missing configuration keys: OrganisationAppUrl.");
    }

    [Fact]
    public async Task ExecuteWorkAsync_ShouldThrowError_WhenMouReminderToSignEmailTemplateIdConfigNotSet()
    {
        List<MouReminderOrganisation> organisations =
            [new() { Id = 1, Name = "Org1", Email = "test1@example.com,test2@example.com", Guid = Guid.NewGuid() }];

        _mockMouRepository.Setup(r => r.GetMouReminderOrganisations()).ReturnsAsync(organisations);
        _mockMouRepository.Setup(r => r.UpsertMouEmailReminder(1)).Returns(Task.CompletedTask);

        var service = new CompleteMoUReminderService(
            _mockLogger.Object,
            _mockMouRepository.Object,
            new ConfigurationBuilder().AddInMemoryCollection([new("OrganisationAppUrl", "http://baseurl/")]).Build(),
            _mockGovUKNotifyClient.Object);

        Func<Task> act = async () => await service.ExecuteWorkAsync(CancellationToken.None);

        await act.Should()
           .ThrowAsync<Exception>()
           .WithMessage("Missing configuration keys: GOVUKNotify:MouReminderToSignEmailTemplateId.");
    }
}