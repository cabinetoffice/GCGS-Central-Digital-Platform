using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetConnectedEntitiesUseCase(IConnectedEntityRepository connectedEntityRepository, IMapper mapper)
    : IUseCase<Guid, IEnumerable<Model.ConnectedEntity>>
{
    public async Task<IEnumerable<Model.ConnectedEntity>> Execute(Guid organisationId)
    {
        return await connectedEntityRepository.FindByOrganisation(organisationId)
            .AndThen(mapper.Map<IEnumerable<Model.ConnectedEntity>>);
    }
}