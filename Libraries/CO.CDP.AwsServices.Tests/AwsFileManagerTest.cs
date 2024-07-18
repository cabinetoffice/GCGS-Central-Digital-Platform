using Amazon.S3;
using Amazon.S3.Model;
using CO.CDP.AwsServices.S3;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CO.CDP.AwsServices.Tests;

public class AwsFileManagerTest
{
    private readonly Mock<IAmazonS3> _mockS3Client;
    private readonly Mock<ITransferUtility> _mockTransferUtility;
    private readonly AwsConfiguration _awsConfig;
    private readonly AwsFileManager _fileManager;
    private const string StagingBucket = "test-staging-bucket";
    private const string PermanentBucket = "test-hosting-bucket";

    public AwsFileManagerTest()
    {
        _mockS3Client = new Mock<IAmazonS3>();
        _mockTransferUtility = new Mock<ITransferUtility>();

        _awsConfig = new AwsConfiguration
        {
            AccessKeyId = "test",
            SecretAccessKey = "test",
            Buckets = new Buckets
            {
                StagingBucket = StagingBucket,
                PermanentBucket = PermanentBucket
            },
        };

        var mockOptions = new Mock<IOptions<AwsConfiguration>>();
        mockOptions.Setup(o => o.Value).Returns(_awsConfig);

        _fileManager = new AwsFileManager(_mockS3Client.Object, _mockTransferUtility.Object, mockOptions.Object);
    }

    [Fact]
    public void UploadFile_ThrowsException_ForEmptyFilename()
    {
        using var stream = new MemoryStream(new byte[10]);

        Func<Task> act = async () => await _fileManager.UploadFile(stream, " ");

        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void UploadFile_ThrowsException_ForMissingFileUploadStagingBucketConfigKey()
    {
        using var stream = new MemoryStream(new byte[10]);
        if (_awsConfig.Buckets != null) _awsConfig.Buckets.StagingBucket = null;

        Func<Task> act = async () => await _fileManager.UploadFile(stream, "file.text");

        act.Should().ThrowAsync<Exception>().WithMessage($"Missing Aws configuration 'StagingBucket' key.");
    }

    [Fact]
    public async Task UploadFile_UploadsFileUsingPutObject_WhenFileIsSmall()
    {
        using var stream = new MemoryStream(new byte[10]);
        var filename = "smallfile.txt";

        await _fileManager.UploadFile(stream, filename);

        _mockS3Client.Verify(s3 => s3.PutObjectAsync(It.Is<PutObjectRequest>(req =>
            req.BucketName == StagingBucket &&
            req.Key == filename &&
            req.InputStream == stream &&
            req.ContentType == "application/octet-stream"
        ), default), Times.Once);
    }

    [Fact]
    public async Task UploadFile_UploadsFileUsingTransferUtility_WhenFileIsLarge()
    {
        using var stream = new MemoryStream(new byte[104857601]);
        var filename = "largefile.txt";

        await _fileManager.UploadFile(stream, filename);

        _mockS3Client.Verify(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), default), Times.Never);
        _mockTransferUtility.Verify(s3 => s3.UploadAsync(It.Is<Amazon.S3.Transfer.TransferUtilityUploadRequest>(req =>
            req.BucketName == StagingBucket &&
            req.Key == filename &&
            req.InputStream == stream &&
            req.ContentType == "application/octet-stream"
        ), default), Times.Once);
    }

    [Fact]
    public void CopyToPermanentBucket_ThrowsException_ForEmptyFilename()
    {
        Func<Task> act = async () => await _fileManager.CopyToPermanentBucket(" ");

        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void CopyToPermanentBucket_ThrowsException_ForMissingFileUploadStagingBucketConfigKey()
    {
        if (_awsConfig.Buckets != null) _awsConfig.Buckets.StagingBucket = null;

        Func<Task> act = async () => await _fileManager.CopyToPermanentBucket("file.text");

        act.Should().ThrowAsync<Exception>().WithMessage($"Missing Aws configuration 'StagingBucket' key.");
    }

    [Fact]
    public void CopyToPermanentBucket_ThrowsException_ForMissingFileHostingBucketConfigKey()
    {
        if (_awsConfig.Buckets != null) _awsConfig.Buckets.PermanentBucket = null;

        Func<Task> act = async () => await _fileManager.CopyToPermanentBucket("file.text");

        act.Should().ThrowAsync<Exception>().WithMessage($"Missing Aws configuration 'PermanentBucket' key.");
    }

    [Fact]
    public async Task CopyToPermanentBucket_CopiesFile_WhenFileIsSmall()
    {
        var filename = "smallfile.txt";
        _mockS3Client.Setup(s3 => s3.GetObjectMetadataAsync(It.IsAny<GetObjectMetadataRequest>(), default))
                     .ReturnsAsync(new GetObjectMetadataResponse { ContentLength = 10 });

        await _fileManager.CopyToPermanentBucket(filename);

        _mockS3Client.Verify(s3 => s3.CopyObjectAsync(It.Is<CopyObjectRequest>(req =>
            req.SourceBucket == StagingBucket &&
            req.DestinationBucket == PermanentBucket &&
            req.SourceKey == filename &&
            req.DestinationKey == filename
        ), default), Times.Once);

        _mockS3Client.Verify(s3 => s3.DeleteObjectAsync(It.Is<DeleteObjectRequest>(req =>
            req.BucketName == StagingBucket &&
            req.Key == filename
        ), default), Times.Once);
    }

    [Fact]
    public async Task CopyToPermanentBucket_CopiesFile_WhenFileIsLarge()
    {
        var filename = "largefile.txt";
        _mockS3Client.Setup(s3 => s3.GetObjectMetadataAsync(It.IsAny<GetObjectMetadataRequest>(), default))
                     .ReturnsAsync(new GetObjectMetadataResponse { ContentLength = 104857601 });

        _mockS3Client.Setup(s3 => s3.InitiateMultipartUploadAsync(It.IsAny<InitiateMultipartUploadRequest>(), default))
                     .ReturnsAsync(new InitiateMultipartUploadResponse { UploadId = "uploadId" });

        _mockS3Client.Setup(s3 => s3.CopyPartAsync(It.IsAny<CopyPartRequest>(), default))
                     .ReturnsAsync(new CopyPartResponse { ETag = "etag" });

        await _fileManager.CopyToPermanentBucket(filename);

        _mockS3Client.Verify(s3 => s3.CopyObjectAsync(It.IsAny<CopyObjectRequest>(), default), Times.Never);
        _mockS3Client.Verify(s3 => s3.InitiateMultipartUploadAsync(It.IsAny<InitiateMultipartUploadRequest>(), default), Times.Once);
        _mockS3Client.Verify(s3 => s3.CopyPartAsync(It.IsAny<CopyPartRequest>(), default), Times.AtLeastOnce);
        _mockS3Client.Verify(s3 => s3.CompleteMultipartUploadAsync(It.IsAny<CompleteMultipartUploadRequest>(), default), Times.Once);

        _mockS3Client.Verify(s3 => s3.DeleteObjectAsync(It.Is<DeleteObjectRequest>(req =>
            req.BucketName == StagingBucket &&
            req.Key == filename
        ), default), Times.Once);
    }
}