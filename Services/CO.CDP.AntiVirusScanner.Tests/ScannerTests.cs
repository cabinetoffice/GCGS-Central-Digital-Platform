using CO.CDP.AwsServices;
using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;

namespace CO.CDP.AntiVirusScanner.Tests;

public class ScannerTests
{
    private readonly Mock<IFileHostManager> _fileHostManagerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<Scanner>> _loggerMock;
    private readonly Scanner _scanner;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<IGovUKNotifyApiClient> _govUKNotifyApiClient;

    public ScannerTests()
    {
        _fileHostManagerMock = new Mock<IFileHostManager>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<Scanner>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _govUKNotifyApiClient = new Mock<IGovUKNotifyApiClient>();
        _scanner = new Scanner(_fileHostManagerMock.Object,
            _configurationMock.Object,
            _loggerMock.Object,
            _govUKNotifyApiClient.Object,
            new HttpClient(_httpMessageHandlerMock.Object));
    }

    [Fact]
    public async Task Scan_IfFileScannedAndClean_CopyFileToPermanentBucket()
    {
        ScanFile fileToScan = GetScanFile();
        var fileStream = new MemoryStream();
        _fileHostManagerMock.Setup(m => m.DownloadStagingFile(fileToScan.QueueFileName)).ReturnsAsync(fileStream);
        _configurationMock.Setup(c => c["ClamAvScanUrl"]).Returns("http://clamav-rest:9000/scan");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Scan successful")
            });

        await _scanner.Scan(fileToScan);

        _fileHostManagerMock.Verify(m => m.CopyToPermanentBucket(fileToScan.QueueFileName, /* deleteInSource */ true), Times.Once);
        _govUKNotifyApiClient.Verify(c => c.SendEmail(It.IsAny<EmailNotificationRequest>()), Times.Never);
    }

    [Fact]
    public async Task Scan_IfFileScannedAndNotClean_RemoveFromStagingBuckett()
    {
        var fileToScan = GetScanFile();
        var fileStream = new MemoryStream();
        _fileHostManagerMock.Setup(m => m.DownloadStagingFile(fileToScan.QueueFileName)).ReturnsAsync(fileStream);
        _configurationMock.Setup(c => c["ClamAvScanUrl"]).Returns("http://clamav-rest:9000/scan");
        _configurationMock.Setup(c => c["GOVUKNotify:FileContainedVirusEmailTemplateId"]).Returns("00000000-0000-0000-0000-000000000000");
        _configurationMock.Setup(c => c["OrganisationAppUrl"]).Returns("http://localhost:8090");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Scan failed")
            });

        await _scanner.Scan(fileToScan);

        _fileHostManagerMock.Verify(m => m.CopyToPermanentBucket(fileToScan.QueueFileName, It.IsAny<bool>()), Times.Never);
        _fileHostManagerMock.Verify(m => m.RemoveFromStagingBucket(fileToScan.QueueFileName), Times.Once);
        _govUKNotifyApiClient.Verify(c => c.SendEmail(It.IsAny<EmailNotificationRequest>()), Times.Exactly(2));
        _loggerMock.VerifyLog(LogLevel.Error, "File scan failed", Times.AtLeastOnce());
    }

    [Fact]
    public async Task Scan_ShouldRemoveFileAndSendEmail_WhenVirusIsDetected()
    {
        var fileToScan = GetScanFile();
        var fileStream = new MemoryStream(Encoding.UTF8.GetBytes("Fake PDF Content"));

        _configurationMock.Setup(c => c["ClamAvScanUrl"]).Returns("http://clamav-rest:9000/scan");
        _configurationMock.Setup(c => c["GOVUKNotify:FileContainedVirusEmailTemplateId"]).Returns("00000000-0000-0000-0000-000000000000");
        _configurationMock.Setup(c => c["OrganisationAppUrl"]).Returns("http://localhost:8090");
        _fileHostManagerMock.Setup(m => m.DownloadStagingFile(fileToScan.QueueFileName)).ReturnsAsync(fileStream);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotAcceptable,
                Content = new StringContent("Virus Found")
            });

        await _scanner.Scan(fileToScan);

        _fileHostManagerMock.Verify(m => m.RemoveFromStagingBucket(fileToScan.QueueFileName), Times.Once);
        _loggerMock.VerifyLog(LogLevel.Information, "File scan failed - virus is found", Times.AtLeastOnce());
    }

    private static ScanFile GetScanFile()
    {
        return new ScanFile
        {
            FullName = "John Smith",
            OrganisationEmailAddress = "admin@acme.org",
            OrganisationName = "Acme Org",
            UserEmailAddress = "john.smith@acme.org",
            UploadedFileName = "testfile.txt",
            QueueFileName = "testfile_20250120112720805.txt",
            OrganisationId = Guid.NewGuid()
        };
    }
}

public static class LoggerMockExtensions
{
    public static void VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel level, string message, Times times)
    {
        logger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == level),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            times
        );
    }
}