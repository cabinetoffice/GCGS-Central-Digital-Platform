using AutoMapper;
using CO.CDP.OrganisationInformation.Persistence;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetConnectedEntitiesUseCase(IConnectedEntityRepository connectedEntityRepository, IMapper mapper)
    : IUseCase<Guid, IEnumerable<Model.ConnectedEntityLookup>>
{
    public async Task<IEnumerable<Model.ConnectedEntityLookup>> Execute(Guid organisationId)
    {
        var entities = await connectedEntityRepository.GetSummary(organisationId);

        entities = entities.Where(ce => !ce!.Deleted);

        IEnumerable<Model.ConnectedEntityLookup>? mappedEntities = mapper.Map<IEnumerable<Model.ConnectedEntityLookup>>(entities, o =>
        {
            o.Items["OrganisationId"] = organisationId;
        });

        foreach(var entity in mappedEntities)
        {
            if (entity != null)
            {
                var connectedEntity = await connectedEntityRepository.IsConnectedEntityUsedInExclusionAsync(
                   organisationId,
                   entity.EntityId);

                entity.IsInUse = connectedEntity.Item1;
                entity.FormGuid = connectedEntity.Item2;
                entity.SectionGuid = connectedEntity.Item3;
            }
        }

        return mappedEntities;
    }
}