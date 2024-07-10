using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class UpdateConnectedEntityUseCase(
    IConnectedEntityRepository connectedEntityRepository,
    IMapper mapper,
    Func<Guid> guidFactory)
    : IUseCase<(Guid organisationId, UpdateConnectedEntity updateConnectedEntity), bool>
{
    public UpdateConnectedEntityUseCase(
        IConnectedEntityRepository connectedEntityRepository,
        IMapper mapper)
        : this(connectedEntityRepository, mapper, Guid.NewGuid)
    {
    }

    public async Task<bool> Execute((Guid organisationId, UpdateConnectedEntity updateConnectedEntity) command)
    {
        var connectedEntity = CreateConnectedEntity(command.updateConnectedEntity);

        await connectedEntityRepository.Save(connectedEntity);

        return await Task.FromResult(true);
    }

    private OrganisationInformation.Persistence.ConnectedEntity
        CreateConnectedEntity(UpdateConnectedEntity command)
    {
        var connectedEntity = MapRequestToConnectedEntity(command.ConnectedEntity);

        return connectedEntity;
    }

    private OrganisationInformation.Persistence.ConnectedEntity
        MapRequestToConnectedEntity(Model.ConnectedEntity command) =>
        mapper.Map<OrganisationInformation.Persistence.ConnectedEntity>(command, o =>
        {
            o.Items["Guid"] = guidFactory();
        });
}