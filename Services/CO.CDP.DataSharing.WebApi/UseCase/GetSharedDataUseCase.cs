using AutoMapper;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class GetSharedDataUseCase(
    IShareCodeRepository shareCodeRepository,
    IMapper mapper)
    : IUseCase<string, SupplierInformation?>
{
    public async Task<SupplierInformation?> Execute(string sharecode)
    {
        var sharedConsent = await shareCodeRepository.GetByShareCode(sharecode);
        if (sharedConsent == null)
        {
            throw new SharedConsentNotFoundException("Shared Consent not found.");
        }

        return mapper.Map<SupplierInformation>(sharedConsent);
    }
}