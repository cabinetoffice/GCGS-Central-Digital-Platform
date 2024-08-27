using AutoMapper;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class GetShareCodeDetailsUseCase(IShareCodeRepository shareCodeRepository, IMapper mapper)
    : IUseCase<(Guid organisationId, string shareCode), SharedConsentDetails?>
{
    public async Task<SharedConsentDetails?> Execute((Guid organisationId, string shareCode) input)
    {
        var details = await shareCodeRepository.GetShareCodeDetailsAsync(input.organisationId, input.shareCode);

        if (details == null) return null;       

        return mapper.Map<SharedConsentDetails>(details);
    }
}