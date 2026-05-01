using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CO.CDP.OrganisationInformation.Persistence;

public class DatabaseConnectedEntityRepository(OrganisationInformationContext context) : IConnectedEntityRepository
{
    private static readonly JsonSerializerOptions JsonReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private sealed record ExclusionAnswer(Guid Id, string? Type);

    private IQueryable<ActiveExclusionAnswerRow> ActiveExclusionAnswers(Guid organisationId) =>
        (from fas in context.FormAnswerSets.AsNoTracking()
         join fs in context.Set<FormSection>() on fas.SectionId equals fs.Id
         join f in context.Forms on fs.FormId equals f.Id
         join sc in context.SharedConsents on new { fs.FormId, Id = fas.SharedConsentId } equals new { sc.FormId, sc.Id }
         join fa in context.Set<FormAnswer>() on fas.Id equals fa.FormAnswerSetId
         where fas.Deleted == false
               && fs.Type == FormSectionType.Exclusions
               && fa.JsonValue != null
               && sc.Organisation.Guid == organisationId
               && !context.FormAnswerSets.Any(d => d.Deleted && d.CreatedFrom == fas.Guid)
         select new ActiveExclusionAnswerRow { FormGuid = f.Guid, SectionGuid = fs.Guid, JsonValue = fa.JsonValue! });

    private sealed class ActiveExclusionAnswerRow
    {
        public Guid FormGuid { get; init; }
        public Guid SectionGuid { get; init; }
        public string JsonValue { get; init; } = string.Empty;
    }

    private static ExclusionAnswer? TryDeserialiseExclusionAnswer(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<ExclusionAnswer>(json, JsonReadOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

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

    public async Task<Tuple<bool, Guid, Guid>> IsConnectedEntityUsedInExclusionAsync(Guid organisationId, Guid connectedEntityId)
    {
        var entityIdJson = $$"""{"id":"{{connectedEntityId}}"}""";

        var match = await ActiveExclusionAnswers(organisationId)
            .Where(r => EF.Functions.JsonContains(r.JsonValue, entityIdJson))
            .Select(r => new { r.FormGuid, r.SectionGuid })
            .FirstOrDefaultAsync();

        return match != null
            ? new Tuple<bool, Guid, Guid>(true, match.FormGuid, match.SectionGuid)
            : new Tuple<bool, Guid, Guid>(false, Guid.Empty, Guid.Empty);
    }

    public async Task<IReadOnlyDictionary<Guid, Tuple<Guid, Guid>>> GetConnectedEntityExclusionUsageAsync(Guid organisationId)
    {
        var rows = await ActiveExclusionAnswers(organisationId).ToListAsync();

        var result = new Dictionary<Guid, Tuple<Guid, Guid>>();

        foreach (var row in rows)
        {
            if (string.IsNullOrEmpty(row.JsonValue)) continue;

            var answer = TryDeserialiseExclusionAnswer(row.JsonValue);
            if (answer == null || answer.Id == Guid.Empty) continue;

            // Preserve "first match wins" semantics of the original implementation.
            result.TryAdd(answer.Id, new Tuple<Guid, Guid>(row.FormGuid, row.SectionGuid));
        }

        return result;
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