using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using CO.CDP.AwsServices.S3;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace CO.CDP.AwsServices.Tests.S3;

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
            Credentials = new Credentials
            {
                AccessKeyId = "test",
                SecretAccessKey = "test"
            },
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

        var contentLength = await GetObjectSize(StagingBucket, filename);
        contentLength.Should().Be(10);
    }

    [Fact]
    public async Task UploadFile_UploadsFileUsingTransferUtility_WhenFileIsLarge()
    {
        using var stream = new MemoryStream(new byte[104857601]);
        var filename = "largefile.txt";
        await _s3Client.PutBucketAsync(new PutBucketRequest { BucketName = StagingBucket });

        await _fileManager.UploadFile(stream, filename);

        var contentLength = await GetObjectSize(StagingBucket, filename);
        contentLength.Should().Be(104857601);
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

        Func<Task> act = async () => await GetObjectSize(StagingBucket, filename);
        var error = await act.Should().ThrowAsync<AmazonS3Exception>();
        error.Which.ErrorCode.Should().Be("NotFound");
        var contentLength = await GetObjectSize(PermanentBucket, filename);
        contentLength.Should().Be(10);
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

        Func<Task> act = async () => await GetObjectSize(StagingBucket, filename);
        var error = await act.Should().ThrowAsync<AmazonS3Exception>();
        error.Which.ErrorCode.Should().Be("NotFound");
        var contentLength = await GetObjectSize(PermanentBucket, filename);
        contentLength.Should().Be(104857601);
    }

    [Fact]
    public void RemoveFromPermanentBucket_ThrowsException_ForEmptyFilename()
    {
        Func<Task> act = async () => await _fileManager.RemoveFromPermanentBucket(" ");

        act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("filename");
    }

    [Fact]
    public void RemoveFromPermanentBucket_ThrowsException_ForMissingPermanentBucketConfigKey()
    {
        if (_awsConfig.Buckets != null) _awsConfig.Buckets.PermanentBucket = null;

        Func<Task> act = async () => await _fileManager.RemoveFromPermanentBucket("file.text");

        act.Should().ThrowAsync<Exception>().WithMessage($"Missing Aws configuration 'PermanentBucket' key.");
    }

    [Fact]
    public async Task RemoveFromPermanentBucket_DeletesFile_WhenExists()
    {
        var filename = "smallfile.txt";
        await _s3Client.PutBucketAsync(new PutBucketRequest { BucketName = StagingBucket });
        await _s3Client.PutBucketAsync(new PutBucketRequest { BucketName = PermanentBucket });
        using var stream = new MemoryStream(new byte[10]);
        await _fileManager.UploadFile(stream, filename);
        await _fileManager.CopyToPermanentBucket(filename);

        await _fileManager.RemoveFromPermanentBucket(filename);

        Func<Task> act = async () => await GetObjectSize(PermanentBucket, filename);
        var error = await act.Should().ThrowAsync<AmazonS3Exception>();
        error.Which.ErrorCode.Should().Be("NotFound");
    }

    [Fact]
    public void GeneratePresignedUrl_ThrowsException_ForEmptyFilename()
    {
        Func<Task> act = async () => await _fileManager.GeneratePresignedUrl(" ", 1);

        act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("filename");
    }

    [Fact]
    public void GeneratePresignedUrl_ThrowsException_ForMissingPermanentBucketConfigKey()
    {
        if (_awsConfig.Buckets != null) _awsConfig.Buckets.PermanentBucket = null;

        Func<Task> act = async () => await _fileManager.GeneratePresignedUrl("file.text", 1);

        act.Should().ThrowAsync<Exception>().WithMessage($"Missing Aws configuration 'PermanentBucket' key.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7*24*60+1)]
    public void GeneratePresignedUrl_ShouldThrowArgumentExceptopm(int urlExpiryInMinutes)
    {
        Func<Task> act = async () => await _fileManager.GeneratePresignedUrl("file.text", urlExpiryInMinutes);

        act.Should().ThrowAsync<ArgumentException>().WithMessage("'urlExpiryInMinutes' can only be between 1 to 10080.");
    }

    [Fact]
    public async Task GeneratePresignedUrl_CreatesUrl_WhenFileExists()
    {
        var filename = "smallfile.txt";
        await _s3Client.PutBucketAsync(new PutBucketRequest { BucketName = StagingBucket });
        await _s3Client.PutBucketAsync(new PutBucketRequest { BucketName = PermanentBucket });
        using var stream = new MemoryStream(new byte[10]);
        await _fileManager.UploadFile(stream, filename);
        await _fileManager.CopyToPermanentBucket(filename);

        var preSignedUrl = await _fileManager.GeneratePresignedUrl(filename, 1);

        preSignedUrl.Should().NotBeNull();
        Uri.TryCreate(preSignedUrl, UriKind.Absolute, out _).Should().BeTrue();
    }

    private async Task<long> GetObjectSize(string bucketName, string filename)
    {
        var metadataResponse = await _s3Client.GetObjectMetadataAsync(
                new GetObjectMetadataRequest
                {
                    BucketName = bucketName,
                    Key = filename
                });

        return metadataResponse.ContentLength; // Length in bytes.
    }
}