using Amazon.S3.Transfer;

namespace CO.CDP.AwsServices.S3;

public interface ITransferUtility
{
    Task UploadAsync(TransferUtilityUploadRequest request, CancellationToken cancellationToken = default);
}