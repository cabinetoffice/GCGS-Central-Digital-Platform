using CO.CDP.EntityFrameworkCore.DbContext;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
              .Include(b => b.BuyerInfo)
              .Include(s => s.SupplierInfo)
             .Include(p => p.Addresses)
             .ThenInclude(p => p.Address)
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
            .Include(p => p.Tenant)
            .Include(p => p.Addresses)
            .ThenInclude(p => p.Address)
            .AsSingleQuery()
            .FirstOrDefaultAsync(t => t.Guid == organisationId);
    }

    public async Task<Organisation?> FindByName(string name)
    {
        return await context.Organisations
            .Include(p => p.Addresses)
            .ThenInclude(p => p.Address)
            .AsSingleQuery()
            .FirstOrDefaultAsync(t => t.Name == name);
    }

    public async Task<IEnumerable<Organisation>> SearchByName(string name, PartyRole? role, int? limit)
    {
        var query = context.Organisations
            .Include(p => p.Addresses)
            .ThenInclude(p => p.Address)
            .AsSingleQuery()
            .Where(t => t.PendingRoles.Count == 0)
            .Select(t => new
            {
                Organisation = t,
                SimilarityScore = EF.Functions.TrigramsSimilarity(t.Name, name)
            })
            .Where(t => t.SimilarityScore > 0.3);

        if (role.HasValue)
        {
            query = query.Where(t => t.Organisation.Roles.Contains(role.Value));
        }

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        query.OrderByDescending(t => t.SimilarityScore);

        return await query.Select(t => t.Organisation).ToListAsync();
    }

    public async Task<IEnumerable<OrganisationPerson>> FindOrganisationPersons(Guid organisationId)
    {
        return await context.Set<OrganisationPerson>()
            .Include(op => op.Person)
            .Where(op => op.Organisation != null && op.Organisation.Guid == organisationId)
            .AsSingleQuery()
            .ToListAsync();
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

    public async Task<IEnumerable<Organisation>> FindByUserUrn(string userUrn)
    {
        var person = await context.Persons
            .Include(p => p.Organisations)
            .ThenInclude(p => p.Addresses)
            .ThenInclude(p => p.Address)
            .AsSingleQuery()
            .FirstOrDefaultAsync(p => p.UserUrn == userUrn);
        return person?.Organisations ?? [];
    }

    public async Task<Organisation?> FindByIdentifier(string scheme, string identifierId)
    {
        return await context.Organisations
            .Include(p => p.Identifiers)
            .Include(p => p.Addresses)
            .ThenInclude(p => p.Address)
            .AsSingleQuery()
            .FirstOrDefaultAsync(o => o.Identifiers.Any(i => i.Scheme == scheme && i.IdentifierId == identifierId));
    }

    public async Task<IList<Organisation>> Get(string? type)
    {
        IQueryable<Organisation> result = context.Organisations
            .Include(o => o.ReviewedBy)
            .Include(o => o.Identifiers)
            .Include(o => o.BuyerInfo)
            .Include(o => o.SupplierInfo)
            .Include(o => o.Addresses)
            .ThenInclude(p => p.Address);

        switch (type)
        {
            case "buyer":
                result = result.Where(o => o.Roles.Contains(PartyRole.Buyer) || o.PendingRoles.Contains(PartyRole.Buyer));
                break;
            case "supplier":
                result = result.Where(o => o.Roles.Contains(PartyRole.Tenderer));
                break;
        }

        return await result.AsSingleQuery().ToListAsync();
    }

    public async Task<IList<Organisation>> GetPaginated(PartyRole? role, PartyRole? pendingRole, string? searchText, int limit, int skip)
    {
        IQueryable<Organisation> result = context.Organisations
            .Include(o => o.ReviewedBy)
            .Include(o => o.Identifiers)
            .Include(o => o.BuyerInfo)
            .Include(o => o.SupplierInfo)
            .Include(o => o.Persons)
            .ThenInclude(p => p.PersonOrganisations)
            .Include(o => o.Addresses)
            .Include(o => o.Addresses)
            .ThenInclude(p => p.Address)
            .OrderBy(o => o.Name)
            .Where(o =>
                (pendingRole.HasValue && o.PendingRoles.Contains(pendingRole.Value)) ||
                (role.HasValue && o.Roles.Contains(role.Value))
            );

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            result = result.Where(o => EF.Functions.TrigramsSimilarity(o.Name, searchText) > 0.3);
        }

        return await result
            .AsSingleQuery()
            .Skip(skip)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<int> GetTotalCount(PartyRole? role, PartyRole? pendingRole, string? searchText)
    {
        IQueryable<Organisation> result = context.Organisations
            .Where(o =>
                (pendingRole.HasValue && o.PendingRoles.Contains(pendingRole.Value)) ||
                (role.HasValue && o.Roles.Contains(role.Value))
            );

        if (searchText != null)
        {
            result = result.Where(o => EF.Functions.TrigramsSimilarity(o.Name, searchText) > 0.3);
        }

        return await result.CountAsync();
    }

    public async Task<IList<ConnectedEntity>> GetConnectedIndividualTrusts(int organisationId)
    {
        var result = context.ConnectedEntities
            .Include(x => x.IndividualOrTrust)
            .Where(x => x.IndividualOrTrust != null && x.EntityType == ConnectedEntity.ConnectedEntityType.Individual)
            .Where(x => x.EndDate == null || x.EndDate > DateTime.Today)
            .Where(x => x.SupplierOrganisation != null && x.SupplierOrganisation.Id == organisationId);

        return await result.AsSingleQuery().ToListAsync();
    }

    public async Task<IList<ConnectedEntity>> GetConnectedOrganisations(int organisationId)
    {
        var result = context.ConnectedEntities
            .Include(x => x.Organisation)
            .Where(x => x.Organisation != null && x.EntityType == ConnectedEntity.ConnectedEntityType.Organisation)
            .Where(x => x.EndDate == null || x.EndDate > DateTime.Today)
            .Where(x => x.SupplierOrganisation != null && x.SupplierOrganisation.Id == organisationId);

        return await result.AsSingleQuery().ToListAsync();
    }

    public async Task<IList<ConnectedEntity>> GetConnectedTrustsOrTrustees(int organisationId)
    {
        var result = context.ConnectedEntities
            .Include(x => x.IndividualOrTrust)
            .Where(x => x.IndividualOrTrust != null && x.EntityType == ConnectedEntity.ConnectedEntityType.TrustOrTrustee)
            .Where(x => x.EndDate == null || x.EndDate > DateTime.Today)
            .Where(x => x.SupplierOrganisation != null && x.SupplierOrganisation.Id == organisationId);

        return await result.AsSingleQuery().ToListAsync();
    }

    public async Task<Organisation.LegalForm?> GetLegalForm(int organisationId)
    {
        var organisation = await context.Organisations
            .Where(x => x.Id == organisationId && x.SupplierInfo != null)
            .Include(x => x.SupplierInfo)
                .ThenInclude(x => x != null ? x.LegalForm : null)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return organisation?.SupplierInfo?.LegalForm;
    }

    public async Task<IList<OperationType>> GetOperationTypes(int organisationId)
    {
        var organisation = await context.Organisations
            .Where(x => x.Id == organisationId)
            .Include(x => x.SupplierInfo)
            .AsSingleQuery()
            .FirstOrDefaultAsync();

        return organisation?.SupplierInfo?.OperationTypes ?? [];
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
