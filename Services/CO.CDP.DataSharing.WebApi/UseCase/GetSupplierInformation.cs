using AutoMapper;
using CO.CDP.DataSharing.WebApi.Extensions;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class UpdateSharedConsentWithShareCodeUseCase(IFormRepository formRepository, IMapper mapper)
    : IUseCase<ShareRequest, ShareReceipt>
{
    public async Task<ShareReceipt> Execute(ShareRequest shareRequest)
    {
        var result = await formRepository.GetSharedConsentAsync(shareRequest.FormId, shareRequest.OrganisationId);
        if(result == null)
        {
            return null;
        }

        var shareCode = ShareCodeExtensions.GenerateShareCode();
        result.BookingReference = shareCode;
        result.UpdatedOn = DateTime.Now;
        await formRepository.SaveSharedConsentAsync(result);

        return mapper.Map<SharedConsent, ShareReceipt>(result);
    }
}