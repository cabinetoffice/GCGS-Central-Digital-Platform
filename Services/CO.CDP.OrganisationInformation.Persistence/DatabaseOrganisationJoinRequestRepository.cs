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

    public async Task<OrganisationJoinRequest?> Find(Guid organisationJoinRequestId, Guid organisationId)
    {
        return await context.OrganisationJoinRequests
            .Include(ojr => ojr.Organisation)
            .Include(ojr => ojr.Person)
            .FirstOrDefaultAsync(t => t.Organisation!.Guid == organisationId && t.Guid == organisationJoinRequestId);
    }

    public async Task<IEnumerable<OrganisationJoinRequest>> FindByOrganisation(Guid organisationId)
    {
        return await context.OrganisationJoinRequests
            .Include(ojr => ojr.Person)
            .Where(ojr => ojr.Organisation != null && ojr.Organisation.Guid == organisationId)
            .ToArrayAsync();
    }

    public async Task<OrganisationJoinRequest?> FindByOrganisationAndPerson(Guid organisationId, int personId)
    {
        return await context.OrganisationJoinRequests
            .FirstOrDefaultAsync(ojr => ojr.Organisation != null && ojr.Organisation.Guid == organisationId && ojr.PersonId == personId);
    }

    public void Save(OrganisationJoinRequest organisationJoinRequest)
    {
        context.Update(organisationJoinRequest);
        context.SaveChanges();
    }
}