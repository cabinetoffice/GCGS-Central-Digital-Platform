using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

public class DatabaseAnnouncementRepository(OrganisationInformationContext context) : IAnnouncementRepository
{
    public async Task<List<Announcement>> GetActiveAnnouncementsAsync(string page)
    {
        var now = DateTimeOffset.UtcNow;

        var announcements = await context.Announcements
            .Where(a =>
                (a.StartDate == null || a.StartDate <= now) &&
                (a.EndDate == null || a.EndDate >= now))
            .OrderByDescending(a => a.StartDate ?? DateTimeOffset.MinValue)
            .ToListAsync();

        return announcements
            .Where(a =>
                string.IsNullOrWhiteSpace(a.UrlRegex) ||
                Regex.IsMatch(page, a.UrlRegex, RegexOptions.IgnoreCase))
            .ToList();
    }

    public void Dispose()
    {
        context.Dispose();
    }
}