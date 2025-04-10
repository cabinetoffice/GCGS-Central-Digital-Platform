namespace CO.CDP.OrganisationInformation.Persistence;

public interface IAnnouncementRepository : IDisposable
{
    public void Save(Announcement announcement);

    public Task<Announcement> GetActiveAnnouncementAsync(string page, string scope);
}