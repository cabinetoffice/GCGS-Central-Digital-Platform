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

    public async Task<Organisation?> Find(int organisationId)
    {
        return await context.Organisations
            .Include(p => p.Addresses)
            .ThenInclude(p => p.Address)
            .FirstOrDefaultAsync(t => t.Id == organisationId);
    }

    public async Task<Organisation?> FindByName(string name)
    {
        return await context.Organisations
            .Include(p => p.Addresses)
            .ThenInclude(p => p.Address)
            .FirstOrDefaultAsync(t => t.Name == name);
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