using FluentAssertions;
using CO.CDP.Testcontainers.PostgreSql;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class DatabaseAnnouncementRepositoryTests(PostgreSqlFixture postgreSql)
    : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task GetActiveAnnouncementsAsync_ReturnsMatchingAnnouncements()
    {
        using var repository = AnnouncementRepository();
        using var context = GetDbContext();

        var now = DateTimeOffset.UtcNow;

        var matchingAnnouncement = new Announcement
        {
            Title = "Matching",
            Body = "This should match",
            StartDate = now.AddDays(-1),
            EndDate = now.AddDays(1),
            UrlRegex = "^/Organisation/(OrganisationOverview|OrganisationSelection)$",
            Guid = Guid.NewGuid()
        };

        var nonMatchingAnnouncement = new Announcement
        {
            Title = "Non Matching",
            Body = "Should not match",
            StartDate = now.AddDays(-1),
            EndDate = now.AddDays(1),
            UrlRegex = "^/OtherPage$",
            Guid = Guid.NewGuid()
        };

        var expiredAnnouncement = new Announcement
        {
            Title = "Expired",
            Body = "Should not match (expired)",
            StartDate = now.AddDays(-10),
            EndDate = now.AddDays(-5),
            UrlRegex = "^/Organisation/OrganisationOverview$",
            Guid = Guid.NewGuid()
        };

        await context.Announcements.AddRangeAsync(matchingAnnouncement, nonMatchingAnnouncement, expiredAnnouncement);
        await context.SaveChangesAsync();

        var results = await repository.GetActiveAnnouncementsAsync("/Organisation/OrganisationOverview");

        results.Should().ContainSingle(); // Only one announcement should match
        results.First().Title.Should().Be("Matching");
    }

    [Fact]
    public async Task GetActiveAnnouncementsAsync_ReturnsEmpty_WhenNoMatches()
    {
        using var repository = AnnouncementRepository();
        using var context = GetDbContext();

        var now = DateTimeOffset.UtcNow;

        var announcement = new Announcement
        {
            Title = "No Match",
            Body = "Wrong page",
            StartDate = now.AddDays(-1),
            EndDate = now.AddDays(1),
            UrlRegex = "^/NotMatching$",
            Guid = Guid.NewGuid()
        };

        await context.Announcements.AddAsync(announcement);
        await context.SaveChangesAsync();

        var results = await repository.GetActiveAnnouncementsAsync("/DifferentPage");

        results.Should().BeEmpty();
    }

    private DatabaseAnnouncementRepository AnnouncementRepository() => new(GetDbContext());

    private OrganisationInformationContext? context = null;

    private OrganisationInformationContext GetDbContext()
    {
        return context ?? postgreSql.OrganisationInformationContext();
    }
}
