using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

public class DatabaseOrganisationJoinRequestRepository(OrganisationInformationContext context) : IOrganisationJoinRequestRepository
{
    public void Dispose()
    {
        context.Dispose();
    }

    public async Task<OrganisationJoinRequest?> Find(Guid organisationJoinRequestId)
    {
        return await context.OrganisationJoinRequests
            .Include(ojr => ojr.Organisation)
            .Include(ojr => ojr.Person)
            .FirstOrDefaultAsync(t => t.Guid == organisationJoinRequestId);
    }

    public async Task<IEnumerable<OrganisationJoinRequest>> FindByOrganisation(Guid organisationJoinRequestId)
    {
        return await context.OrganisationJoinRequests
            .Include(ojr => ojr.Person)
            .Where(ojr => ojr.Organisation.Guid == organisationJoinRequestId)
            .ToArrayAsync();
    }

    public void Save(OrganisationJoinRequest organisationJoinRequest)
    {
        context.Update(organisationJoinRequest);
        context.SaveChanges();
    }
}