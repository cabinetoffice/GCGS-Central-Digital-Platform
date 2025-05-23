using CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp;

public class AnnouncementService(IOrganisationClient organisationClient) : IAnnouncementService
{
    public async Task<Announcement?> GetLatestAnnouncementAsync(string? page)
    {
        var announcements = await organisationClient.GetAnnouncementsAsync(page);

        return announcements.FirstOrDefault();
    }
}