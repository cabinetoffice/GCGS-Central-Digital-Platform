using CO.CDP.EntityFrameworkCore.DbContext;
using Microsoft.EntityFrameworkCore;

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
            .Include(p => p.Addresses)
            .ThenInclude(p => p.Address)
            .AsSingleQuery()
            .FirstOrDefaultAsync(t => t.Guid == organisationId);
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

    public async Task<IList<ConnectedEntity>> GetConnectedIndividualTrusts(int organisationId)
    {
        var result = context.ConnectedEntities
            .Include(x => x.IndividualOrTrust)
            .Where(x => x.IndividualOrTrust != null && x.EntityType == ConnectedEntity.ConnectedEntityType.Individual)
            .Where(x => x.SupplierOrganisation != null && x.SupplierOrganisation.Id == organisationId);

        return await result.AsSingleQuery().ToListAsync();
    }

    public async Task<IList<ConnectedEntity>> GetConnectedOrganisations(int organisationId)
    {
        var result = context.ConnectedEntities
            .Include(x => x.Organisation)
            .Where(x => x.Organisation != null && x.EntityType == ConnectedEntity.ConnectedEntityType.Organisation)
            .Where(x => x.SupplierOrganisation != null && x.SupplierOrganisation.Id == organisationId);

        return await result.AsSingleQuery().ToListAsync();
    }

    public async Task<IList<ConnectedEntity>> GetConnectedTrustsOrTrustees(int organisationId)
    {
        var result = context.ConnectedEntities
            .Include(x => x.IndividualOrTrust)
            .Where(x => x.IndividualOrTrust != null && x.EntityType == ConnectedEntity.ConnectedEntityType.TrustOrTrustee)
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
}