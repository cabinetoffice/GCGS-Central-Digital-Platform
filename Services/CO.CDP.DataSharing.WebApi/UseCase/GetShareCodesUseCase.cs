using AutoMapper;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class GetShareCodesUseCase(
    IFormRepository formRepository,
    IMapper mapper)
    : IUseCase<Guid, List<SharedConsent>?>
{

    public async Task<List<SharedConsent>?> Execute(Guid organisationId)
    {
        return await formRepository.GetShareCodesAsync(organisationId)
              .AndThen(mapper.Map<List<Model.SharedConsent>>);
    }
}