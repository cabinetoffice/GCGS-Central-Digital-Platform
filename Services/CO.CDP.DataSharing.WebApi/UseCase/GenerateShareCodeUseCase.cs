using AutoMapper;
using CO.CDP.DataSharing.WebApi.Extensions;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class GenerateShareCodeUseCase(IFormRepository formRepository, IMapper mapper)
    : IUseCase<ShareRequest, ShareReceipt>
{
    public async Task<ShareReceipt> Execute(ShareRequest shareRequest)
    {
        var result = await formRepository.GetSharedConsentDraftAsync(shareRequest.FormId, shareRequest.OrganisationId);
        if(result == null)
        {
            throw new SharedConsentNotFoundException("Shared Consent not found.");
        }

        var shareCode = ShareCodeExtensions.GenerateShareCode();
        result.BookingReference = shareCode;
        result.SubmittedAt = DateTime.UtcNow;
        result.SubmissionState = SubmissionState.Submitted;

        await formRepository.SaveSharedConsentAsync(result);

        return mapper.Map<SharedConsent, ShareReceipt>(result);
    }
}