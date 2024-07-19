using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Options;

namespace CO.CDP.AwsServices.S3;

public class AwsFileManager(
        IAmazonS3 s3Client,
        ITransferUtility transferUtility,
        IOptions<AwsConfiguration> awsConfigurationOption) : IFileHostManager
{
    private readonly AwsConfiguration awsConfig = awsConfigurationOption.Value;
    private const int MinMultipartSize = 104857600; // 100MB https://docs.aws.amazon.com/AmazonS3/latest/userguide/mpuoverview.html

    public async Task UploadFile(Stream fileStream, string filename, string contentType = "application/octet-stream")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filename);

        var bucketName = awsConfig.Buckets?.StagingBucket
            ?? throw new Exception($"Missing Aws configuration '{nameof(awsConfig.Buckets.StagingBucket)}' key.");

        if (fileStream.Length < MinMultipartSize)
        {
            await s3Client.PutObjectAsync(
             new PutObjectRequest
             {
                 BucketName = bucketName,
                 Key = filename,
                 InputStream = fileStream,
                 ContentType = contentType
             });
        }
        else
        {
            TransferUtilityUploadRequest fileUploadRequest = new()
            {
                BucketName = bucketName,
                Key = filename,
                PartSize = 5242880, // 5 MB
                InputStream = fileStream,
                ContentType = contentType
            };

            //fileUploadRequest.UploadProgressEvent += (s, e) => Console.WriteLine("{0}/{1}", e.TransferredBytes, e.TotalBytes);
            await transferUtility.UploadAsync(fileUploadRequest);
        }
    }

    public async Task CopyToPermanentBucket(string filename, bool deleteInSource = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filename);

        var sourceBucketName = awsConfig.Buckets?.StagingBucket
            ?? throw new Exception($"Missing Aws configuration '{nameof(awsConfig.Buckets.StagingBucket)}' key.");

        var destBucketName = awsConfig.Buckets?.PermanentBucket
            ?? throw new Exception($"Missing Aws configuration '{nameof(awsConfig.Buckets.PermanentBucket)}' key.");

        var objectSize = await GetObjectSize(sourceBucketName, filename);

        if (objectSize < MinMultipartSize)
        {
            await s3Client.CopyObjectAsync(
                new CopyObjectRequest()
                {
                    SourceBucket = sourceBucketName,
                    DestinationBucket = destBucketName,
                    SourceKey = filename,
                    DestinationKey = filename
                });
        }
        else
        {
            await CopyLargeObjectAsync(sourceBucketName, destBucketName, filename, objectSize);
        }

        if (deleteInSource)
        {
            await s3Client.DeleteObjectAsync(
                new DeleteObjectRequest
                {
                    BucketName = sourceBucketName,
                    Key = filename
                });
        }
    }

    private async Task CopyLargeObjectAsync(string sourceBucketName, string destBucketName, string filename, long objectSize)
    {
        var initResponse = await s3Client.InitiateMultipartUploadAsync(
            new InitiateMultipartUploadRequest
            {
                BucketName = destBucketName,
                Key = filename
            });

        long partSize = 5 * (long)Math.Pow(2, 20); // Part size is 5 MB.
        long bytePosition = 0;
        List<CopyPartResponse> copyResponses = [];

        for (int i = 1; bytePosition < objectSize; i++)
        {
            var copyRequest = new CopyPartRequest
            {
                DestinationBucket = destBucketName,
                DestinationKey = filename,
                SourceBucket = sourceBucketName,
                SourceKey = filename,
                UploadId = initResponse.UploadId,
                FirstByte = bytePosition,
                LastByte = bytePosition + partSize - 1 >= objectSize ? objectSize - 1 : bytePosition + partSize - 1,
                PartNumber = i
            };

            copyResponses.Add(await s3Client.CopyPartAsync(copyRequest));

            bytePosition += partSize;
        }

        var completeRequest = new CompleteMultipartUploadRequest
        {
            BucketName = destBucketName,
            Key = filename,
            UploadId = initResponse.UploadId
        };
        completeRequest.AddPartETags(copyResponses);

        await s3Client.CompleteMultipartUploadAsync(completeRequest);
    }

    private async Task<long> GetObjectSize(string bucketName, string filename)
    {
        var metadataResponse = await s3Client.GetObjectMetadataAsync(
                new GetObjectMetadataRequest
                {
                    BucketName = bucketName,
                    Key = filename
                });

        return metadataResponse.ContentLength; // Length in bytes.
    }
}