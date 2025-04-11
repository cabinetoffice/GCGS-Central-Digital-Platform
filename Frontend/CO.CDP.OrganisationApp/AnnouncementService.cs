using CO.CDP.Organisation.WebApiClient;
using Announcement = CO.CDP.Organisation.WebApiClient.Announcement;

namespace CO.CDP.OrganisationApp;

public class AnnouncementService(IOrganisationClient organisationClient) : IAnnouncementService
{
    public async Task<Announcement> GetLatestAnnouncement(string page)
    {
        return await organisationClient.GetLatestAnnouncementAsync(page);
    }
}
