using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using Announcement = CO.CDP.Organisation.WebApi.Model.Announcement;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class GetAnnouncementsUseCase(IAnnouncementRepository announcementRepository)
    : IUseCase<GetAnnouncementQuery, IEnumerable<Announcement>>
{
    public async Task<IEnumerable<Announcement>> Execute(GetAnnouncementQuery request)
    {
        var announcements = await announcementRepository.GetActiveAnnouncementsAsync(request.Page);

        return announcements.Select(a => new Announcement
        {
            Title = a.Title,
            Body = a.Body,
            StartDate = a.StartDate,
            EndDate = a.EndDate
        });
    }
}