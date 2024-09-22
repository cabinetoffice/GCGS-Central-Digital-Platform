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
            .FirstOrDefaultAsync(t => t.Guid == organisationId);
    }

    public async Task<Organisation?> FindByName(string name)
    {
        return await context.Organisations
            .Include(p => p.Addresses)
            .ThenInclude(p => p.Address)
            .FirstOrDefaultAsync(t => t.Name == name);
    }


    public async Task<OrganisationPerson?> FindOrganisationPerson(Guid organisationId, Guid personId)
    {
        return await context.Set<OrganisationPerson>().FirstOrDefaultAsync(o => o.Organisation.Guid == organisationId && o.Person.Guid == personId);
    }

    public async Task<OrganisationPerson?> FindOrganisationPerson(Guid organisationId, string userUrn)
    {
        return await context.Set<OrganisationPerson>()
            .FirstOrDefaultAsync(o => o.Organisation.Guid == organisationId && o.Person.UserUrn == userUrn);
    }

    public async Task<IEnumerable<Organisation>> FindByUserUrn(string userUrn)
    {
        var person = await context.Persons
            .Include(p => p.Organisations)
            .ThenInclude(p => p.Addresses)
            .ThenInclude(p => p.Address)
            .FirstOrDefaultAsync(p => p.UserUrn == userUrn);
        return person?.Organisations ?? [];
    }

    public async Task<Organisation?> FindByIdentifier(string scheme, string identifierId)
    {
        return await context.Organisations
            .Include(p => p.Identifiers)
            .Include(p => p.Addresses)
            .ThenInclude(p => p.Address)
            .FirstOrDefaultAsync(o => o.Identifiers.Any(i => i.Scheme == scheme && i.IdentifierId == identifierId));
    }

    public async Task<IList<ConnectedEntity>> GetConnectedIndividualTrusts(int organisationId)
    {
        var result = context.ConnectedEntities
            .Include(x => x.IndividualOrTrust)
            .Where(x => x.IndividualOrTrust != null && x.EntityType == ConnectedEntity.ConnectedEntityType.Individual)
            .Where(x => x.SupplierOrganisation != null && x.SupplierOrganisation.Id == organisationId);

        return await result.ToListAsync();
    }

    public async Task<IList<ConnectedEntity>> GetConnectedOrganisations(int organisationId)
    {
        var result = context.ConnectedEntities
            .Include(x => x.Organisation)
            .Where(x => x.Organisation != null && x.EntityType == ConnectedEntity.ConnectedEntityType.Organisation)
            .Where(x => x.SupplierOrganisation != null && x.SupplierOrganisation.Id == organisationId);

        return await result.ToListAsync();
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