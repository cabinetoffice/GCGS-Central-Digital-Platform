using CO.CDP.AwsServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;

namespace CO.CDP.AntiVirusScanner.Tests;

public class ScannerTests
{
    private readonly Mock<IFileHostManager> _fileHostManagerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<Scanner>> _loggerMock;
    private readonly Scanner _scanner;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

    public ScannerTests()
    {
        _fileHostManagerMock = new Mock<IFileHostManager>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<Scanner>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _scanner = new Scanner(_fileHostManagerMock.Object,
            _configurationMock.Object,
            _loggerMock.Object,
            new HttpClient(_httpMessageHandlerMock.Object));
    }

    [Fact]
    public async Task Scan_IfFileScannedAndClean_CopyFileToPermanentBucket()
    {
        var fileToScan = new ScanFile { FileName = "testfile.txt", OrganisationId = Guid.NewGuid() };
        var fileStream = new MemoryStream();
        _fileHostManagerMock.Setup(m => m.DownloadStagingFile(fileToScan.FileName)).ReturnsAsync(fileStream);
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

        _fileHostManagerMock.Verify(m => m.CopyToPermanentBucket(fileToScan.FileName, true), Times.Once);
    }

    [Fact]
    public async Task Scan_IfFileScannedAndNotClean_RemoveFromStagingBuckett()
    {
        var fileToScan = new ScanFile { FileName = "testfile.txt", OrganisationId = Guid.NewGuid() };
        var fileStream = new MemoryStream();
        _fileHostManagerMock.Setup(m => m.DownloadStagingFile(fileToScan.FileName)).ReturnsAsync(fileStream);
        _configurationMock.Setup(c => c["ClamAvScanUrl"]).Returns("http://clamav-rest:9000/scan");

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

        _fileHostManagerMock.Verify(m => m.RemoveFromStagingBucket(fileToScan.FileName), Times.Once);
    }
}