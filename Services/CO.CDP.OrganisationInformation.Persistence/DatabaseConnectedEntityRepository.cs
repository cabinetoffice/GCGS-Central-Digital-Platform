using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

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

    public async Task<IEnumerable<ConnectedEntity?>> FindByOrganisation(Guid organisationId)
    {
        return await context.ConnectedEntities
            .Include(p => p.Addresses)
            .ThenInclude(p => p.Address)
            .Where(t => t.SupplierOrganisation.Guid == organisationId && (t.EndDate == null || t.EndDate > DateTime.Today))
            .ToArrayAsync();
    }

    public Task<bool> IsConnectedEntityUsedInExclusionAsync(Guid organisationId, Guid connectedEntityId)
    {
        var activeOrganisationExclusions = (from fas in context.FormAnswerSets
                                            join fs in context.Set<FormSection>() on fas.SectionId equals fs.Id
                                            join sc in context.SharedConsents on fs.FormId equals sc.FormId
                                            join fa in context.Set<FormAnswer>() on fas.Id equals fa.FormAnswerSetId
                                            where !fas.Deleted &&
                                             fs.Type == FormSectionType.Exclusions &&
                                             fa.JsonValue != null &&
                                             sc.Organisation.Guid == organisationId
                                            select new { sc.FormId, fas.SectionId, SharedConsentId = sc.Id, fa.JsonValue })
                                            .Distinct().ToList();

        foreach (var fas in activeOrganisationExclusions)
        {
            if (!string.IsNullOrEmpty(fas.JsonValue))
            {
                dynamic json = JObject.Parse(fas.JsonValue);

                if (json.id == connectedEntityId)
                {
                    return Task.FromResult(true);
                }
            }
        }

        return Task.FromResult(false);
    }

    public async Task<IEnumerable<ConnectedEntityLookup?>> GetSummary(Guid organisationId)
    {
        var now = DateTimeOffset.UtcNow;
        return await context.ConnectedEntities
            .Where(t => t.SupplierOrganisation.Guid == organisationId && (t.EndDate > now || t.EndDate == null))
            .Select(t => new ConnectedEntityLookup
            {
                Name = t.EntityType == ConnectedEntity.ConnectedEntityType.Organisation
                    ? (t.Organisation == null ? "" : t.Organisation.Name)
                    : (t.IndividualOrTrust == null ? "" : $"{t.IndividualOrTrust.FirstName} {t.IndividualOrTrust.LastName}"),
                EntityId = t.Guid,
                EntityType = t.EntityType,
            })
            .ToArrayAsync();
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