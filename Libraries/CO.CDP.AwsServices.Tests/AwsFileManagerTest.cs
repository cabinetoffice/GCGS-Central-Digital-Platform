using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using CO.CDP.AwsServices.S3;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CO.CDP.AwsServices.Tests;

public class AwsFileManagerTest : IClassFixture<LocalStackFixture>
{
    private readonly IAmazonS3 _s3Client;
    private readonly AwsConfiguration _awsConfig;
    private readonly AwsFileManager _fileManager;
    private const string StagingBucket = "test-staging-bucket";
    private const string PermanentBucket = "test-hosting-bucket";

    public AwsFileManagerTest(LocalStackFixture localStack)
    {
        _s3Client = new AmazonS3Client(
            new BasicAWSCredentials("test", "test"),
            new AmazonS3Config { ServiceURL = localStack.ConnectionString });

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

        _fileManager = new AwsFileManager(_s3Client, new TransferUtilityWrapper(_s3Client), mockOptions.Object);
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
        await _s3Client.PutBucketAsync(new PutBucketRequest { BucketName = StagingBucket });

        await _fileManager.UploadFile(stream, filename);
    }

    [Fact]
    public async Task UploadFile_UploadsFileUsingTransferUtility_WhenFileIsLarge()
    {
        using var stream = new MemoryStream(new byte[104857601]);
        var filename = "largefile.txt";
        await _s3Client.PutBucketAsync(new PutBucketRequest { BucketName = StagingBucket });

        await _fileManager.UploadFile(stream, filename);
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
        await _s3Client.PutBucketAsync(new PutBucketRequest { BucketName = StagingBucket });
        await _s3Client.PutBucketAsync(new PutBucketRequest { BucketName = PermanentBucket });
        using var stream = new MemoryStream(new byte[10]);
        await _fileManager.UploadFile(stream, filename);

        await _fileManager.CopyToPermanentBucket(filename);
    }

    [Fact]
    public async Task CopyToPermanentBucket_CopiesFile_WhenFileIsLarge()
    {
        var filename = "largefile.txt";
        await _s3Client.PutBucketAsync(new PutBucketRequest { BucketName = StagingBucket });
        await _s3Client.PutBucketAsync(new PutBucketRequest { BucketName = PermanentBucket });
        using var stream = new MemoryStream(new byte[104857601]);
        await _fileManager.UploadFile(stream, filename);

        await _fileManager.CopyToPermanentBucket(filename);
    }
}