using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using ConnectedEntity = CO.CDP.OrganisationInformation.Persistence.ConnectedEntity;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class UpdateConnectedEntityUseCase(
    IConnectedEntityRepository connectedEntityRepository,
    IOrganisationRepository organisationRepository,
    IMapper mapper)
    : IUseCase<(Guid organisationId, Guid connectedEntityId, UpdateConnectedEntity updateConnectedEntity), bool>
{
    public async Task<bool> Execute((Guid organisationId, Guid connectedEntityId, UpdateConnectedEntity updateConnectedEntity) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
                                   ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var connectedEntity = await connectedEntityRepository.Find(command.organisationId, command.connectedEntityId)
                              ?? throw new UnknownConnectedEntityException(
                                  $"Unknown connected entity {command.connectedEntityId}.");

        connectedEntity = MapRequestToConnectedEntity(command.updateConnectedEntity, connectedEntity);

        await connectedEntityRepository.Save(connectedEntity);

        return await Task.FromResult(true);
    }

    private OrganisationInformation.Persistence.ConnectedEntity
        MapRequestToConnectedEntity(UpdateConnectedEntity command, ConnectedEntity connectedEntity) =>
        mapper.Map(command, connectedEntity);
}