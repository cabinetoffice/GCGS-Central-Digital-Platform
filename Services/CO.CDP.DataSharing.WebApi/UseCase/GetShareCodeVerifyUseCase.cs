using CO.CDP.Authentication;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class GetShareCodeVerifyUseCase(
    IShareCodeRepository shareCodeRepository,
    IClaimService claimService)
    : IUseCase<ShareVerificationRequest,
        ShareVerificationReceipt>
{
    public async Task<ShareVerificationReceipt> Execute(ShareVerificationRequest shareRequest)
    {
        var organisationId = claimService.GetOrganisationId();
        if (organisationId == null || await shareCodeRepository.OrganisationShareCodeExistsAsync(organisationId.Value, shareRequest.ShareCode) == false)
        {
            throw new UserUnauthorizedException();
        }

        var details = await shareCodeRepository.GetShareCodeVerifyAsync(shareRequest.FormVersionId, shareRequest.ShareCode);

        if (details == null)
        {
            throw new ShareCodeNotFoundException(Constants.ShareCodeNotFoundExceptionMessage);
        }

        return new ShareVerificationReceipt
        {
            FormVersionId = shareRequest.FormVersionId,
            ShareCode = shareRequest.ShareCode,
            IsLatest = details.Value
        };
    }
}