using AutoMapper;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetConnectedEntitiesUseCase(IConnectedEntityRepository connectedEntityRepository, IMapper mapper)
    : IUseCase<Guid, IEnumerable<Model.ConnectedEntityLookup>>
{
    public async Task<IEnumerable<Model.ConnectedEntityLookup>> Execute(Guid organisationId)
    {
        var entities = await connectedEntityRepository.GetSummary(organisationId);
        return mapper.Map<IEnumerable<Model.ConnectedEntityLookup>>(entities, o =>
        {
            o.Items["OrganisationId"] = organisationId;            
        });
    }
}