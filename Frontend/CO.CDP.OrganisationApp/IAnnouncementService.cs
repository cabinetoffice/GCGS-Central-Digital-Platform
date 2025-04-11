using CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp;

public interface IAnnouncementService
{
    Task<Announcement> GetLatestAnnouncement(string page);
}