using CO.CDP.EntityFrameworkCore.DbContext;
using CO.CDP.OrganisationInformation.Persistence.Constants;
using CO.CDP.OrganisationInformation.Persistence.NonEfEntities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace CO.CDP.OrganisationInformation.Persistence;

public class DatabaseOrganisationRepository(OrganisationInformationContext context) : IOrganisationRepository
{
    public void Dispose()
    {
        context.Dispose();
    }

    public async Task<Organisation?> Find(Guid organisationId)
    {
        return await context.Organisations
                .Include(b => b.Identifiers)
                .Include(b => b.ContactPoints)
                .Include(b => b.BuyerInfo)
                .Include(s => s.SupplierInfo)
                .Include(p => p.Addresses).ThenInclude(p => p.Address)
                .AsSingleQuery()
                .FirstOrDefaultAsync(t => t.Guid == organisationId);
    }

    public async Task<Organisation?> FindIncludingPersons(Guid organisationId)
    {
        return await context.Organisations
            .Include(o => o.Tenant)
            .ThenInclude(t => t.Persons)
            .Include(o => o.OrganisationPersons)
            .ThenInclude(op => op.Person)
            .AsSingleQuery()
            .FirstOrDefaultAsync(t => t.Guid == organisationId);
    }

    public async Task<Organisation?> FindIncludingReviewedBy(Guid organisationId)
    {
        return await context.Organisations
            .Include(o => o.ReviewedBy)
            .AsSingleQuery()
            .FirstOrDefaultAsync(t => t.Guid == organisationId);
    }

    public async Task<Organisation?> FindIncludingTenantByOrgId(int id)
    {
        return await context.Organisations
            .Include(p => p.Tenant)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Organisation?> FindIncludingTenant(Guid organisationId)
    {
        return await context.Organisations
                .Include(b => b.Identifiers)
                .Include(b => b.ContactPoints)
                .Include(b => b.BuyerInfo)
                .Include(s => s.SupplierInfo)
                .Include(p => p.Tenant)
                .Include(p => p.Addresses)
                .ThenInclude(p => p.Address)
                .AsSingleQuery()
                .FirstOrDefaultAsync(t => t.Guid == organisationId);
    }

    public async Task<Organisation?> FindByName(string name)
    {
        return await context.Organisations
                .Include(b => b.Identifiers)
                .Include(b => b.ContactPoints)
                .Include(b => b.BuyerInfo)
                .Include(s => s.SupplierInfo)
                .Include(p => p.Addresses)
                .ThenInclude(p => p.Address)
                .AsSingleQuery()
                .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
    }

    public async Task<IEnumerable<Organisation>> SearchByName(string name, PartyRole? role, int? limit, double threshold = 0.3, bool includePendingRoles = false)
    {
        var query = context.Organisations
            .Include(b => b.Identifiers)
            .Include(p => p.Addresses)
            .ThenInclude(p => p.Address)
            .AsSingleQuery()
            .Select(t => new
            {
                Organisation = t,
                SimilarityScore = EF.Functions.TrigramsSimilarity(t.Name, name)
            })
            .Where(t => t.SimilarityScore >= threshold)
            .Where(t =>
                    t.Organisation.Type == OrganisationType.Organisation ||
                    context.OrganisationParties
                        .Count(op => op.ParentOrganisationId == t.Organisation.Id) >= 2
                );

        if (role.HasValue)
        {
            if (includePendingRoles == true)
            {
                query = query.Where(t => t.Organisation.Roles.Contains(role.Value) || t.Organisation.PendingRoles.Contains(role.Value));
            }
            else
            {
                query = query.Where(t => t.Organisation.Roles.Contains(role.Value));
            }
        }
        else if (includePendingRoles == false)
        {
            query = query.Where(t => t.Organisation.PendingRoles.Count == 0);
        }

        query = query.OrderByDescending(t => t.SimilarityScore);

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return await query.Select(t => t.Organisation).ToListAsync();
    }

    public async Task<(IEnumerable<Organisation> Results, int TotalCount)> SearchByNameOrPpon(string searchText, int? limit, int skip, string orderBy, double threshold = 0.3)
    {
        return await SearchByNameOrPpon(searchText, limit, skip, orderBy, threshold, false);
    }

    public async Task<(IEnumerable<Organisation> Results, int TotalCount)> SearchByNameOrPpon(string searchText, int? limit, int skip, string orderBy, double threshold, bool excludeOnlyPendingBuyerRoles)
    {
        var baseQuery = context.Organisations
            .Include(b => b.Identifiers)
            .Include(p => p.Addresses)
            .ThenInclude(p => p.Address)
            .AsSingleQuery()
            .Where(t =>
                EF.Functions.TrigramsSimilarity(t.Name, searchText) >= threshold ||
                t.Identifiers.Any(i => i.IdentifierId != null && i.IdentifierId.Equals(searchText)))
            .Where(t => t.Type == OrganisationType.Organisation)
            .Where(t => t.Identifiers.Any(i => i.Scheme.Equals("GB-PPON")));

        if (excludeOnlyPendingBuyerRoles)
        {
            baseQuery = baseQuery.Where(t =>
                !t.PendingRoles.Contains(PartyRole.Buyer) ||
                t.Roles.Any(r => r != PartyRole.Buyer) ||
                t.Roles.Contains(PartyRole.Buyer)
            );
        }

        baseQuery = orderBy.ToLowerInvariant() switch
        {
            "asc" => baseQuery.OrderBy(t => t.Name),
            "desc" => baseQuery.OrderByDescending(t => t.Name),
            _ => baseQuery.OrderByDescending(t => EF.Functions.TrigramsSimilarity(t.Name, searchText)).ThenBy(t => t.Name)
        };

        int totalCount = await baseQuery.CountAsync();

        var results = limit.HasValue
            ? await baseQuery.Skip(skip).Take(limit.Value).ToListAsync()
            : await baseQuery.Skip(skip).ToListAsync();

        return (results, totalCount);
    }

    public async Task<IEnumerable<Organisation>> FindByOrganisationEmail(string email, PartyRole? role, int? limit)
    {
        var query = context.Organisations
            .Include(b => b.Identifiers)
            .Include(o => o.Addresses)
            .ThenInclude(oa => oa.Address)
            .AsSingleQuery()
            .Where(o => o.PendingRoles.Count == 0 && o.ContactPoints.Any(cp => cp.Email == email));

        if (role.HasValue)
        {
            query = query.Where(o => o.Roles.Contains(role.Value));
        }

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Organisation>> FindByAdminEmail(string email, PartyRole? role, int? limit)
    {
        var organisations = await context.Organisations
            .Include(b => b.Identifiers)
            .Include(o => o.Persons)
            .ThenInclude(p => p.PersonOrganisations)
            .Include(o => o.Addresses)
            .ThenInclude(oa => oa.Address)
            .AsSingleQuery()
            .Where(o => o.PendingRoles.Count == 0
                        && o.Persons.Any(p => p.Email == email))
            .ToListAsync();

        var filteredOrganisations = organisations
            .Where(o => o.Persons.Any(p =>
                p.PersonOrganisations.Any(po => po.Scopes.Contains(OrganisationPersonScopes.Admin) && o.Id == po.OrganisationId)
            ));

        if (role.HasValue)
        {
            filteredOrganisations = filteredOrganisations.Where(o => o.Roles.Contains(role.Value));
        }

        if (limit.HasValue)
        {
            filteredOrganisations = filteredOrganisations.Take(limit.Value);
        }

        return filteredOrganisations;
    }

    public async Task<IEnumerable<OrganisationPerson>> FindOrganisationPersons(Guid organisationId, IEnumerable<string>? anyScopes = null)
    {
        var people = await context.Set<OrganisationPerson>()
            .Include(op => op.Person)
            .Where(op => op.Organisation != null && op.Organisation.Guid == organisationId)
            .AsSingleQuery().ToArrayAsync();

        if (anyScopes?.Any() == true)
        {
            people = people.Where(q => anyScopes.Any(scope => q.Scopes.Any(s => s == scope))).ToArray();
        }

        return people;
    }

    public async Task<OrganisationPerson?> FindOrganisationPerson(Guid organisationId, Guid personId)
    {
        return await context.Set<OrganisationPerson>().FirstOrDefaultAsync(o => o.Organisation != null && o.Organisation.Guid == organisationId && o.Person.Guid == personId);
    }

    public async Task<OrganisationPerson?> FindOrganisationPerson(Guid organisationId, string userUrn)
    {
        return await context.Set<OrganisationPerson>()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Organisation != null && o.Organisation.Guid == organisationId && o.Person.UserUrn == userUrn);
    }

    public async Task<Organisation?> FindByIdentifier(string scheme, string identifierId)
    {
        return await context.Organisations
                .Include(b => b.Identifiers)
                .Include(b => b.ContactPoints)
                .Include(b => b.BuyerInfo)
                .Include(s => s.SupplierInfo)
                .Include(p => p.Addresses)
                .ThenInclude(p => p.Address)
                .AsSingleQuery()
                .FirstOrDefaultAsync(o => o.Identifiers.Any(i => i.Scheme == scheme && i.IdentifierId == identifierId));
    }

    public async Task<Tuple<IList<OrganisationRawDto>, int>> GetPaginated(PartyRole? role, PartyRole? pendingRole, string? searchText, int limit, int skip)
    {
        var conditions = new List<string>();

        var roleConditions = new List<string>();
        if (role.HasValue) roleConditions.Add(":role = ANY(o.roles)");
        if (pendingRole.HasValue) roleConditions.Add(":pendingRole = ANY(o.pending_roles)");
        if (roleConditions.Count > 0)
            conditions.Add($"({string.Join(" OR ", roleConditions)})");

        if (!string.IsNullOrWhiteSpace(searchText))
            conditions.Add("(o.name % :searchText OR o.name ILIKE '%' || :searchText || '%')");

        var finalCondition = conditions.Count > 0 ? string.Join(" AND ", conditions) : null;

        var sql = @"
            SELECT
                o.id,
                o.guid,
                o.name,
                o.type,
                o.roles,
                o.pending_roles,
                o.approved_on,
                o.review_comment,
                reviewed_by.first_name AS reviewed_by_first_name,
                reviewed_by.last_name AS reviewed_by_last_name,
                COALESCE(STRING_AGG(DISTINCT i.scheme || ':' || i.identifier_id, ', '), '') AS identifiers,
                COALESCE(STRING_AGG(DISTINCT cp.email, ', '), '') AS contact_points,
                COALESCE(STRING_AGG(DISTINCT p.email, ', '), '') AS admin_email,"
                + (string.IsNullOrWhiteSpace(searchText) ? "0 AS similarity_score, 0 AS match_position" : @"
                    similarity(o.name, :searchText) AS similarity_score,
                    NULLIF(POSITION(LOWER(:searchText) IN LOWER(o.name)), 0) AS match_position") +
            @"
            FROM
                organisations o
                LEFT JOIN organisation_person op ON op.organisation_id = o.id AND op.scopes @> '[""ADMIN""]'::jsonb
                LEFT JOIN persons p ON p.id = op.person_id
                LEFT JOIN identifiers i ON i.organisation_id = o.id
                LEFT JOIN contact_points cp ON cp.organisation_id = o.id
                LEFT JOIN persons reviewed_by ON reviewed_by.id = o.reviewed_by_id"
            + (finalCondition != null ? $" WHERE {finalCondition}" : "") +
            @"
            GROUP BY
                o.id, o.guid, o.name, o.type, o.roles, o.pending_roles, o.approved_on, o.review_comment, reviewed_by.first_name, reviewed_by.last_name
            ORDER BY
                match_position ASC NULLS LAST, similarity_score DESC, o.name ASC";

        var roleValue = role.HasValue ? (int)role.Value : (object)DBNull.Value;
        var pendingRoleValue = pendingRole.HasValue ? (int)pendingRole.Value : (object)DBNull.Value;

        var parameters = new[]
        {
            new NpgsqlParameter("role", NpgsqlDbType.Integer) { Value = roleValue },
            new NpgsqlParameter("pendingRole", NpgsqlDbType.Integer) { Value = pendingRoleValue },
            new NpgsqlParameter("searchText", NpgsqlDbType.Text) { Value = string.IsNullOrWhiteSpace(searchText) ? (object)DBNull.Value : searchText },
            new NpgsqlParameter("limit", NpgsqlDbType.Integer) { Value = limit },
            new NpgsqlParameter("skip", NpgsqlDbType.Integer) { Value = skip }
        };

        var rawResults = await context.Database.SqlQueryRaw<OrganisationRawDto>(sql, parameters).ToListAsync();
        var totalCount = rawResults.Count;

        rawResults = rawResults.Skip(skip)
            .Take(limit)
            .ToList();

        return new Tuple<IList<OrganisationRawDto>, int>(rawResults, totalCount);
    }

    public async Task<bool> IsEmailUniqueWithinOrganisation(Guid organisationId, string email)
    {
        return await context.Organisations
            .Where(x => x.Guid == organisationId)
            .AnyAsync(x => !x.Persons.Any(y => y.Email == email));
    }

    public void Save(Organisation organisation)
    {
        try
        {
            context.Update(organisation);
            context.SaveChanges();
        }
        catch (DbUpdateException cause)
        {
            HandleDbUpdateException(organisation, cause);
        }
    }

    public async Task SaveAsync(Organisation organisation, Func<Organisation, Task> onSave) =>
        await context.InTransaction(async _ =>
        {
            Save(organisation);
            // The assumption here is that the `onSave()` callback uses the same `DbContext` as the current repository.
            await onSave(organisation);
            await context.SaveChangesAsync();
        });

    public void SaveOrganisationPerson(OrganisationPerson organisationPerson)
    {
        context.Update(organisationPerson);
        context.SaveChanges();
    }

    public void SaveOrganisationMou(MouSignature mouSignature)
    {
        context.MouSignature.Update(mouSignature);
        context.SaveChanges();
    }

    private static void HandleDbUpdateException(Organisation organisation, DbUpdateException cause)
    {
        switch (cause.InnerException)
        {
            case { } e when e.Message.Contains("_organisations_name") || e.Message.Contains("_tenants_name"):
                // Currently the Organisation Name matches the Tenant Name.
                // When both tenant and organisation are created at the same time, the Tenant is inserted first,
                // and throws an error before the organisation insert is attempted.
                // It's unexpected to get a duplicate tenant exception from the organisation repository.
                throw new IOrganisationRepository.OrganisationRepositoryException.DuplicateOrganisationException(
                    $"Organisation with name `{organisation.Name}` already exists.", cause);
            case { } e when e.Message.Contains("_organisations_guid"):
                throw new IOrganisationRepository.OrganisationRepositoryException.DuplicateOrganisationException(
                    $"Organisation with guid `{organisation.Guid}` already exists.", cause);
            case { } e when e.Message.Contains("ix_identifiers_identifier_id_scheme"):
                throw new IOrganisationRepository.OrganisationRepositoryException.DuplicateIdentifierException(
                    $"You cannot use this registration number. It is being used by another organisation.", cause);
            default:
                throw cause;
        }
    }

    public async Task<IEnumerable<MouSignature>> GetMouSignatures(int organisationId)
    {
        return await context.MouSignature
        .Where(x => x.OrganisationId == organisationId)
        .Include(m => m.Mou)
        .Include(p => p.CreatedBy)
        .ToListAsync();
    }

    public async Task<MouSignature?> GetMouSignature(int organisationId, Guid mouSignatureId)
    {
        return await context.MouSignature
        .Where(x => x.OrganisationId == organisationId && x.SignatureGuid == mouSignatureId)
        .Include(m => m.Mou)
        .Include(p => p.CreatedBy)
        .FirstOrDefaultAsync();
    }
    public async Task<Mou?> GetLatestMou()
    {
        return await context.Mou
            .OrderByDescending(m => m.CreatedOn)
        .FirstOrDefaultAsync();
    }

    public async Task<Mou?> GetMou(Guid mouId)
    {
        return await context.Mou
                .Where(m => m.Guid == mouId)
        .FirstOrDefaultAsync();
    }
}
