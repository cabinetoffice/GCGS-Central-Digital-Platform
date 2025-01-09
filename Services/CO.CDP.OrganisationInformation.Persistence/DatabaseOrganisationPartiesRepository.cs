using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

public class DatabaseOrganisationPartiesRepository(OrganisationInformationContext context)
    : IOrganisationPartiesRepository
{
    public void Dispose()
    {
        context.Dispose();
    }

    public async Task<IEnumerable<OrganisationParty>> Find(Guid organisationId)
    {
        return await context
            .OrganisationParties
            .Include(x => x.ChildOrganisation)
            .Include(x => x.SharedConsent)
            .Where(x => x.ParentOrganisation!.Guid == organisationId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task Save(OrganisationParty organisationParty)
    {
        context.Update(organisationParty);
        await context.SaveChangesAsync();
    }
}