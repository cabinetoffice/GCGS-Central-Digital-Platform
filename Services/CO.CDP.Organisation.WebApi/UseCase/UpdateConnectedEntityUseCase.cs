using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class UpdateConnectedEntityUseCase(
    IConnectedEntityRepository connectedEntityRepository,
    IMapper mapper,
    Func<Guid> guidFactory)
    : IUseCase<(Guid organisationId, Model.ConnectedEntity updateConnectedEntity), bool>
{
    public UpdateConnectedEntityUseCase(
        IConnectedEntityRepository connectedEntityRepository,
        IMapper mapper)
        : this(connectedEntityRepository, mapper, Guid.NewGuid)
    {
    }

    public async Task<bool> Execute((Guid organisationId, Model.ConnectedEntity updateConnectedEntity) command)
    {
        var connectedEntity = CreateConnectedEntity(command.updateConnectedEntity);

        await connectedEntityRepository.Save(connectedEntity);

        return await Task.FromResult(true);
    }

    private OrganisationInformation.Persistence.ConnectedEntity
        CreateConnectedEntity(Model.ConnectedEntity command)
    {
        var connectedEntity = MapRequestToConnectedEntity(command);

        return connectedEntity;
    }

    private OrganisationInformation.Persistence.ConnectedEntity
        MapRequestToConnectedEntity(Model.ConnectedEntity command) =>
        mapper.Map<OrganisationInformation.Persistence.ConnectedEntity>(command, o =>
        {
            o.Items["Guid"] = guidFactory();
        });
}