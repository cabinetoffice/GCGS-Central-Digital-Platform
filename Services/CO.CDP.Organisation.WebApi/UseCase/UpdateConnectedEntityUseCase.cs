using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class UpdateConnectedEntityUseCase(
    IConnectedEntityRepository connectedEntityRepository,
    IOrganisationRepository organisationRepository,
    IMapper mapper,
    Func<Guid> guidFactory)
    : IUseCase<(Guid organisationId, RegisterConnectedEntity updateConnectedEntity), bool>
{
    public UpdateConnectedEntityUseCase(
        IConnectedEntityRepository connectedEntityRepository,
        IOrganisationRepository organisationRepository,
        IMapper mapper)
        : this(connectedEntityRepository, organisationRepository, mapper, Guid.NewGuid)
    {
    }

    public async Task<bool> Execute((Guid organisationId, RegisterConnectedEntity updateConnectedEntity) command)
    {
        var connectedEntity = MapRequestToConnectedEntity(command.updateConnectedEntity);

        var supplierOrganisation = await organisationRepository.Find(command.organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        connectedEntity.SupplierOrganisation = supplierOrganisation;

        await connectedEntityRepository.Save(connectedEntity);

        return await Task.FromResult(true);
    }

    private OrganisationInformation.Persistence.ConnectedEntity
        MapRequestToConnectedEntity(RegisterConnectedEntity command) =>
        mapper.Map<OrganisationInformation.Persistence.ConnectedEntity>(command, o =>
        {
            o.Items["Guid"] = guidFactory();
        });
}