using CO.CDP.AwsServices;
using Microsoft.Extensions.Configuration;

namespace CO.CDP.AntiVirusScanner;

public interface IScanner
{
    void Scan(ScanFile fileName);
}

public class Scanner(IFileHostManager fileHostManager, IConfiguration configuration) : IScanner
{
    public async void Scan(ScanFile fileToScan)
    {
        try
        {
            var fileAsStream = await fileHostManager.DownloadStagingFile(fileToScan.FileName);
            var fileAsByteArray = StreamToByteArray(fileAsStream);

            using (var httpClient = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    var fileContent = new ByteArrayContent(fileAsByteArray);
                    content.Add(fileContent, "file", fileToScan.FileName);

                    var calmAvUrl = configuration.GetValue<string>("ClamAvScanUrl");
                    var response = httpClient.PostAsync(calmAvUrl, content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        // TODO: log filename and org id

                        var responseContent = response.Content.ReadAsStringAsync().Result;
                        Console.WriteLine(responseContent);

                        await fileHostManager.CopyToPermanentBucket(fileToScan.FileName);
                    }
                    else
                    {
                        await fileHostManager.RemoveFromStagingBucket(fileToScan.FileName);

                        Console.WriteLine("File request did not complete: " + response.StatusCode);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
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
