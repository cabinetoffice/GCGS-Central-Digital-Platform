namespace CO.CDP.AwsServices;

public interface IFileHostManager
{
    Task UploadFile(Stream fileStream, string filename, string contentType);

    Task CopyToPermanentBucket(string filename, bool deleteInSource = true);

    Task RemoveFromPermanentBucket(string filename);

    Task<string> GeneratePresignedUrl(string filename, int urlExpiryInMinutes);
}