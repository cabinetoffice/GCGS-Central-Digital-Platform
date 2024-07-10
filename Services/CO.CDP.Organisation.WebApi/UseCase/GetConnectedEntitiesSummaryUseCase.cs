using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetConnectedEntitiesSummaryUseCase(IConnectedEntityRepository connectedEntityRepository, IMapper mapper)
    : IUseCase<Guid, IEnumerable<Model.ConnectedEntityLookup>>
{
    public async Task<IEnumerable<Model.ConnectedEntityLookup>> Execute(Guid organisationId)
    {
        return await connectedEntityRepository.GetSummary(organisationId)
            .AndThen(mapper.Map<IEnumerable<Model.ConnectedEntityLookup>>);
    }
}