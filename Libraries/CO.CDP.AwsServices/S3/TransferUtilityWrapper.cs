using Amazon.S3;
using Amazon.S3.Transfer;

namespace CO.CDP.AwsServices.S3;

public class TransferUtilityWrapper(IAmazonS3 s3Client) : ITransferUtility
{
    private readonly TransferUtility _transferUtility = new(s3Client);

    public Task UploadAsync(TransferUtilityUploadRequest request, CancellationToken cancellationToken = default)
    {
        return _transferUtility.UploadAsync(request, cancellationToken);
    }
}