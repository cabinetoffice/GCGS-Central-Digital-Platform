namespace CO.CDP.OrganisationInformation.Persistence;

public interface IAnnouncementRepository : IDisposable
{
    public void Save(Announcement announcement);

    public Task<List<Announcement>> GetActiveAnnouncementsAsync(string page);
}