namespace CO.CDP.OrganisationInformation.Persistence;

public class DatabaseAnnouncementRepository(OrganisationInformationContext context) : IAnnouncementRepository
{
    public void Save(Announcement announcement)
    {

    }

    public Announcement GetActiveAnnouncementAsync(string page)
    {
        return new Announcement
        {
            Title = "Test",
            Body = "Body",
            Guid = default
        };
    }

    public void Dispose()
    {
        context.Dispose();
    }
}