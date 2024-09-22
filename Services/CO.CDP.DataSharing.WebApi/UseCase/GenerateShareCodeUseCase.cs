using AutoMapper;
using CO.CDP.DataSharing.WebApi.Extensions;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class GenerateShareCodeUseCase(
    IOrganisationRepository organisationRepository,
    IFormRepository formRepository,
    IShareCodeRepository shareCodeRepository,
    IMapper mapper)
    : IUseCase<ShareRequest, ShareReceipt>
{
    public async Task<ShareReceipt> Execute(ShareRequest shareRequest)
    {
        var org = await organisationRepository.Find(shareRequest.OrganisationId);
        if (org == null)
        {
            throw new InvalidOrganisationRequestedException("Invalid Organisation requested.");
        }

        var result = await shareCodeRepository.GetSharedConsentDraftAsync(shareRequest.FormId, shareRequest.OrganisationId)
            ?? throw new ShareCodeNotFoundException(Constants.ShareCodeNotFoundExceptionMessage);

        var shareCode = ShareCodeExtensions.GenerateShareCode();
        result.ShareCode = shareCode;
        result.SubmittedAt = DateTime.UtcNow;
        result.SubmissionState = SubmissionState.Submitted;

        await formRepository.SaveSharedConsentAsync(result);

        return mapper.Map<OrganisationInformation.Persistence.Forms.SharedConsent, ShareReceipt>(result);
    }
}