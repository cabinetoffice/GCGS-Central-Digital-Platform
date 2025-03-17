using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class DeleteConnectedEntityUseCase(IOrganisationRepository organisationRepository, IConnectedEntityRepository connectedEntityRepository)
            : IUseCase<(Guid organisationId, Guid connectedEntityId, DeleteConnectedEntity deleteConnectedEntity), bool>
{
    public async Task<bool> Execute((Guid organisationId, Guid connectedEntityId, DeleteConnectedEntity deleteConnectedEntity) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var connectedEntity = await connectedEntityRepository.Find(command.organisationId, command.connectedEntityId)
            ?? throw new UnknownConnectedEntityException($"Unknown connected entity {command.connectedEntityId}.");

        var isConnectedPersonInUse = await connectedEntityRepository.IsConnectedEntityUsedInExclusionAsync(
            command.organisationId, command.connectedEntityId);

        if (isConnectedPersonInUse)
        {
            throw new CannotDeleteConnectedEntityException($"Connected entity {command.connectedEntityId} in organisation {command.organisationId} used in active exclusion.");
        }

        connectedEntity.EndDate = command.deleteConnectedEntity.EndDate;

        await connectedEntityRepository.Save(connectedEntity);

        return await Task.FromResult(true);
    }
}