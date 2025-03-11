using CO.CDP.AwsServices;
using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;

namespace CO.CDP.AntiVirusScanner;

public class Scanner(IFileHostManager fileHostManager,
    IConfiguration configuration,
    ILogger<Scanner> logger,
    IGovUKNotifyApiClient govUKNotifyApiClient,
    HttpClient httpClient) : IScanner
{
    public async Task Scan(ScanFile fileToScan)
    {
        try
        {
            var fileAsStream = await fileHostManager.DownloadStagingFile(fileToScan.QueueFileName);
            var fileAsByteArray = StreamToByteArray(fileAsStream);

            using (var content = new MultipartFormDataContent())
            {
                var fileContent = new ByteArrayContent(fileAsByteArray);
                content.Add(fileContent, "file", fileToScan.QueueFileName);

                var calmAvUrl = configuration["ClamAvScanUrl"];
                var response = httpClient.PostAsync(calmAvUrl, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    await fileHostManager.CopyToPermanentBucket(fileToScan.QueueFileName);

                    logger.LogInformation("File scanned successfully: File name: {fileName} OrganisationId: {orgId}", fileToScan.QueueFileName, fileToScan.OrganisationId);
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotAcceptable)
                    {
                        logger.LogInformation("File scan failed - virus is found: {fileName} {orgId}", fileToScan.QueueFileName, fileToScan.OrganisationId);

                        await SendEmail(fileToScan.UserEmailAddress, fileToScan);
                        await SendEmail(fileToScan.OrganisationEmailAddress, fileToScan);
                    }
                    else
                    {
                        logger.LogError("File scan failed: {fileName} {orgId}", fileToScan.QueueFileName, fileToScan.OrganisationId);
                    }                    

                    await fileHostManager.RemoveFromStagingBucket(fileToScan.QueueFileName);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Unexpected error while scanning file: {fileToScan.QueueFileName} {fileToScan.OrganisationId}");
            throw;
        }
    }

    private async Task SendEmail(string emailAddress, ScanFile details)
    {
        var templateId = configuration["GOVUKNotify:FileContainedVirusEmailTemplateId"]
            ?? throw new Exception("Missing configuration key: GOVUKNotify:FileContainedVirusEmailTemplateId.");
        var baseAppUrl = configuration["OrganisationAppUrl"]
            ?? throw new Exception("Missing configuration key: OrganisationAppUrl");
        Uri baseUri = new Uri(baseAppUrl);
        Uri orgDashboard = new Uri(baseUri, $"{details.OrganisationId}");
        var emailRequest = new EmailNotificationRequest
        {
            EmailAddress = emailAddress,
            TemplateId = templateId,
            Personalisation = new Dictionary<string, string> {
                            { "file name", details.UploadedFileName },
                            { "org_link", orgDashboard.ToString() },
                            { "organisation name", details.OrganisationName },
                            { "full_name", details.FullName }
            }
        };

        await govUKNotifyApiClient.SendEmail(emailRequest);
    }

    byte[] StreamToByteArray(Stream stream)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
