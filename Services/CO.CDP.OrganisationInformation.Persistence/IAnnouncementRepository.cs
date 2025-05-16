namespace CO.CDP.OrganisationInformation.Persistence;

public interface IAnnouncementRepository : IDisposable
{
    public Task<List<Announcement>> GetActiveAnnouncementsAsync(string page);
}