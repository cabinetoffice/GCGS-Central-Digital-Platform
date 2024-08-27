using AutoMapper;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class GetShareCodeVerifyUseCase(
    IFormRepository formRepository)
    : IUseCase<ShareVerificationRequest,
        ShareVerificationReceipt>
{
    public async Task<ShareVerificationReceipt> Execute(ShareVerificationRequest shareRequest)
    {
        var details = await formRepository.GetShareCodeVerifyAsync(shareRequest.FormVersionId, shareRequest.ShareCode);

        if (details == null) return null;

        return new ShareVerificationReceipt
        {
            FormVersionId = shareRequest.FormVersionId,
            ShareCode = shareRequest.ShareCode,
            IsLatest = details.Value
        };
    }
}