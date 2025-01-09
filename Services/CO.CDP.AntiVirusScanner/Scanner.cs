using CO.CDP.AwsServices;

namespace CO.CDP.AntiVirusScanner;

public class Scanner(IFileHostManager fileHostManager,
    IConfiguration configuration,
    ILogger<Scanner> logger,
    HttpClient httpClient) : IScanner
{
    public async Task Scan(ScanFile fileToScan)
    {
        try
        {
            var fileAsStream = await fileHostManager.DownloadStagingFile(fileToScan.FileName);
            var fileAsByteArray = StreamToByteArray(fileAsStream);

            using (var content = new MultipartFormDataContent())
            {
                var fileContent = new ByteArrayContent(fileAsByteArray);
                content.Add(fileContent, "file", fileToScan.FileName);

                var calmAvUrl = configuration["ClamAvScanUrl"];
                var response = httpClient.PostAsync(calmAvUrl, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    await fileHostManager.CopyToPermanentBucket(fileToScan.FileName);

                    logger.LogInformation("File scanned successfully: File name: {fileName} OrganisationId: {orgId}", fileToScan.FileName, fileToScan.OrganisationId);
                }
                else
                {
                    await fileHostManager.RemoveFromStagingBucket(fileToScan.FileName);

                    logger.LogInformation("File scan failed: {fileName} {orgId}", fileToScan.FileName, fileToScan.OrganisationId);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
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
