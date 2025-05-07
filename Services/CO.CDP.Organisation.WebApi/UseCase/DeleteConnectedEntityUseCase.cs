using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class DeleteConnectedEntityUseCase(IOrganisationRepository organisationRepository, IConnectedEntityRepository connectedEntityRepository)
            : IUseCase<(Guid organisationId, Guid connectedEntityId), DeleteConnectedEntityResult>
{
    public async Task<DeleteConnectedEntityResult> Execute((Guid organisationId, Guid connectedEntityId) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var connectedEntity = await connectedEntityRepository.Find(command.organisationId, command.connectedEntityId)
            ?? throw new UnknownConnectedEntityException($"Unknown connected entity {command.connectedEntityId}.");

        var result = new DeleteConnectedEntityResult() { Success = true };

        var isConnectedPersonInUse = await connectedEntityRepository.IsConnectedEntityUsedInExclusionAsync(
            command.organisationId, command.connectedEntityId);

        if (isConnectedPersonInUse.Item1)
        {
            result.Success = false;
            result.FormGuid = isConnectedPersonInUse.Item2;
            result.SectionGuid = isConnectedPersonInUse.Item3;
        }
        else
        {
            connectedEntity.Deleted = true;

            await connectedEntityRepository.Save(connectedEntity);
        }

        return await Task.FromResult(result);
    }
}