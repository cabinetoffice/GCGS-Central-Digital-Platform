using AutoMapper;
using CO.CDP.Authentication;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation.Persistence;
using static System.Collections.Specialized.BitVector32;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class GetShareCodeDetailsUseCase(
    IFormRepository formRepository,
    IMapper mapper)
    : IUseCase<(Guid organisationId, string shareCode), SharedConsentQuestionAnswer?>
{

    public async Task<SharedConsentQuestionAnswer?> Execute((Guid organisationId, string shareCode) input)
    {
        var details = await formRepository.GetShareCodeDetailsAsync(input.organisationId, input.shareCode);

        throw new NotImplementedException();
    }
}