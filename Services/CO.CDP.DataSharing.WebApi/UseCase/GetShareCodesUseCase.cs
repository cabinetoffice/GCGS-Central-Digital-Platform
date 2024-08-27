using AutoMapper;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class GetShareCodesUseCase(
    IShareCodeRepository shareCodeRepository,
    IMapper mapper)
    : IUseCase<Guid, List<SharedConsent>?>
{

    public async Task<List<SharedConsent>?> Execute(Guid organisationId)
    {
        return await shareCodeRepository.GetShareCodesAsync(organisationId)
              .AndThen(mapper.Map<List<Model.SharedConsent>>);
    }
}