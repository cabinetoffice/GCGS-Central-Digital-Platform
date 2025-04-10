namespace CO.CDP.OrganisationInformation.Persistence;

public class DatabaseAnnouncementRepository(OrganisationInformationContext context) : IAnnouncementRepository
{
    public void Save(Announcement announcement)
    {

    }

    public Task<Announcement> GetActiveAnnouncementAsync(string page, string scope)
    {

    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}