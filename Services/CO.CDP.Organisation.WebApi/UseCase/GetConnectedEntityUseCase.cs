using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetConnectedEntityUseCase(IConnectedEntityRepository connectedEntityRepository, IMapper mapper)
    : IUseCase<(Guid organisationId, Guid id), Model.ConnectedEntity?>
{
    public async Task<Model.ConnectedEntity?> Execute((Guid organisationId, Guid id) command)
    {
        return await connectedEntityRepository.Find(command.organisationId, command.id)
            .AndThen(mapper.Map<Model.ConnectedEntity>);
    }
}