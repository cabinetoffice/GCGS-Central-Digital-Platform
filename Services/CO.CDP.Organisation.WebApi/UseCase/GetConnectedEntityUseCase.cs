using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetConnectedEntityUseCase(IConnectedEntityRepository connectedEntityRepository, IMapper mapper)
    : IUseCase<Guid, Model.ConnectedEntity?>
{
    public async Task<Model.ConnectedEntity?> Execute(Guid id)
    {
        return await connectedEntityRepository.Find(id)
            .AndThen(mapper.Map<Model.ConnectedEntity>);
    }
}