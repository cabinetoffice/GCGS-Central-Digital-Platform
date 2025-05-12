using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace CO.CDP.OrganisationInformation.Persistence;

public class DatabaseConnectedEntityRepository(OrganisationInformationContext context) : IConnectedEntityRepository
{
    public void Dispose()
    {
        context.Dispose();
    }

    public async Task<ConnectedEntity?> Find(Guid organisationId, Guid id)
    {
        return await context.ConnectedEntities
            .Include(p => p.Addresses)
            .ThenInclude(p => p.Address)
            .FirstOrDefaultAsync(t => t.Guid == id && t.SupplierOrganisation.Guid == organisationId);
    }

    public async Task<IEnumerable<ConnectedEntityLookup?>> GetSummary(Guid organisationId)
    {
        return await context.ConnectedEntities
            .Where(t => t.SupplierOrganisation.Guid == organisationId)
            .Select(t => new ConnectedEntityLookup
            {
                Name = t.EntityType == ConnectedEntityType.Organisation
                    ? (t.Organisation == null ? "" : t.Organisation.Name)
                    : (t.IndividualOrTrust == null ? "" : $"{t.IndividualOrTrust.FirstName} {t.IndividualOrTrust.LastName}"),
                EntityId = t.Guid,
                EntityType = t.EntityType,
                EndDate = t.EndDate,
                Deleted = t.Deleted
            })
            .ToArrayAsync();
    }

    public Task<Tuple<bool, Guid, Guid>> IsConnectedEntityUsedInExclusionAsync(Guid organisationId, Guid connectedEntityId)
    {
        var organisationExclusions = (from fas in context.FormAnswerSets
                                            join fs in context.Set<FormSection>() on fas.SectionId equals fs.Id
                                            join f in context.Forms on fs.FormId equals f.Id
                                            join sc in context.SharedConsents on new { fs.FormId, Id = fas.SharedConsentId } equals new { sc.FormId, sc.Id }
                                            join fa in context.Set<FormAnswer>() on fas.Id equals fa.FormAnswerSetId
                                            where fas.Deleted == false &&
                                                fs.Type == FormSectionType.Exclusions &&
                                                fa.JsonValue != null &&
                                                sc.Organisation.Guid == organisationId 
                                            select new { FormAnswerSetGuid = fas.Guid, FormGuid = f.Guid, SectionGuid = fs.Guid, SharedConsentId = sc.Id, fa.JsonValue, fas.Deleted })
                                            .ToList();
        var connectedEntityInUse = false;

        foreach (var fas in organisationExclusions)
        {
            var isDeleted = from deletedFas in context.FormAnswerSets
                            where deletedFas.Deleted && deletedFas.CreatedFrom == fas.FormAnswerSetGuid
                            select deletedFas;

            if (!isDeleted.Any())
            {
                if (!string.IsNullOrEmpty(fas.JsonValue))
                {
                    dynamic json = JObject.Parse(fas.JsonValue);

                    if (json.id == connectedEntityId)
                    {
                        return Task.FromResult(new Tuple<bool, Guid, Guid>(!connectedEntityInUse, fas.FormGuid, fas.SectionGuid));
                    }
                }
            }
        }

        return Task.FromResult(new Tuple<bool, Guid, Guid>(connectedEntityInUse, Guid.Empty, Guid.Empty));
    }

    public async Task Save(ConnectedEntity connectedEntity)
    {
        try
        {
            context.Update(connectedEntity);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException cause)
        {
            HandleDbUpdateException(connectedEntity, cause);
        }
    }

    private static void HandleDbUpdateException(ConnectedEntity connectedEntity, DbUpdateException cause)
    {
        switch (cause.InnerException)
        {
            case { } e when e.Message.Contains("_connected_entities_guid"):
                throw new IConnectedEntityRepository.ConnectedEntityRepositoryException.DuplicateConnectedEntityException(
                    $"Connected entity with guid `{connectedEntity.Guid}` already exists.", cause);
            default:
                throw cause;
        }
    }
}