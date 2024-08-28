using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class GetShareCodeVerifyUseCase(
    IShareCodeRepository shareCodeRepository)
    : IUseCase<ShareVerificationRequest,
        ShareVerificationReceipt>
{
    public async Task<ShareVerificationReceipt> Execute(ShareVerificationRequest shareRequest)
    {
        var details = await shareCodeRepository.GetShareCodeVerifyAsync(shareRequest.FormVersionId, shareRequest.ShareCode);

        if (details == null)
        {
            throw new SharedConsentNotFoundException("Shared Code not found.");
        }

        return new ShareVerificationReceipt
        {
            FormVersionId = shareRequest.FormVersionId,
            ShareCode = shareRequest.ShareCode,
            IsLatest = details.Value
        };
    }
}