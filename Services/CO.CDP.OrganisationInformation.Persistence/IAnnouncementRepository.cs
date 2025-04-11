namespace CO.CDP.OrganisationInformation.Persistence;

public interface IAnnouncementRepository : IDisposable
{
    public void Save(Announcement announcement);

    public Announcement GetActiveAnnouncementAsync(string page);
}