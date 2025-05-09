using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Announcement = CO.CDP.Organisation.WebApi.Model.Announcement;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class GetAnnouncementsUseCaseTests
{
    private readonly Mock<IAnnouncementRepository> _announcementRepository = new();

    private GetAnnouncementsUseCase _useCase => new GetAnnouncementsUseCase(_announcementRepository.Object);

    [Fact]
    public async Task Execute_ShouldReturnMappedAnnouncements_WhenAnnouncementsExist()
    {
        var page = "/Organisation/OrganisationOverview";

        var announcementEntityList = new List<Persistence.Announcement>
        {
            new()
            {
                Title = "Share code creation unavailable",
                Body = "You will not be able to create a share code between 27th March at 10pm and 28th March at 3am.",
                StartDate = DateTimeOffset.UtcNow.AddDays(-1),
                EndDate = DateTimeOffset.UtcNow.AddDays(1),
                Guid = Guid.NewGuid()
            },
            new()
            {
                Title = "New Feature Release",
                Body = "We have released a new feature!",
                StartDate = DateTimeOffset.UtcNow.AddDays(-2),
                EndDate = DateTimeOffset.UtcNow.AddDays(2),
                Guid = Guid.NewGuid()
            }
        };

        _announcementRepository
            .Setup(repo => repo.GetActiveAnnouncementsAsync(page))
            .ReturnsAsync(announcementEntityList);

        var result = await _useCase.Execute(new GetAnnouncementQuery { Page = page });

        result.Should().HaveCount(2);

        result.Should().BeEquivalentTo(announcementEntityList.Select(a => new Announcement
        {
            Title = a.Title,
            Body = a.Body,
            StartDate = a.StartDate,
            EndDate = a.EndDate
        }));

        _announcementRepository.Verify(repo => repo.GetActiveAnnouncementsAsync(page), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldReturnEmptyList_WhenNoAnnouncementsExist()
    {
        var page = "/Organisation/OrganisationOverview";

        _announcementRepository
            .Setup(repo => repo.GetActiveAnnouncementsAsync(page))
            .ReturnsAsync(new List<Persistence.Announcement>());

        var result = await _useCase.Execute(new GetAnnouncementQuery { Page = page });

        result.Should().BeEmpty();

        _announcementRepository.Verify(repo => repo.GetActiveAnnouncementsAsync(page), Times.Once);
    }
}
