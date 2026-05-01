using AutoMapper;
using CO.CDP.OrganisationInformation.Persistence;

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

        var exclusionUsage = await connectedEntityRepository.GetConnectedEntityExclusionUsageAsync(organisationId);

        foreach (var entity in mappedEntities)
        {
            if (entity == null) continue;

            if (exclusionUsage.TryGetValue(entity.EntityId, out var match))
            {
                entity.IsInUse = true;
                entity.FormGuid = match.Item1;
                entity.SectionGuid = match.Item2;
            }
            else
            {
                entity.IsInUse = false;
                entity.FormGuid = Guid.Empty;
                entity.SectionGuid = Guid.Empty;
            }
        }

        return mappedEntities;
    }
}
