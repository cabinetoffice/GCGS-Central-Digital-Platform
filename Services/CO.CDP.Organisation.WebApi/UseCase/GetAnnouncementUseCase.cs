using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using Announcement = CO.CDP.Organisation.WebApi.Model.Announcement;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class GetAnnouncementUseCase(IAnnouncementRepository announcementRepository)
    : IUseCase<GetAnnouncementQuery, Announcement?>
{
    public async Task<Announcement> Execute(GetAnnouncementQuery request)
    {
        var announcement = await announcementRepository.GetActiveAnnouncementAsync(request.Page, request.Scope);
        return new Announcement()
        {
            Title = announcement.Title,
            Body = announcement.Body,
            StartDate = announcement.StartDate,
            EndDate = announcement.EndDate
        };
    }
}