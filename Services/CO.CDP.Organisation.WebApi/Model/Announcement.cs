namespace CO.CDP.Organisation.WebApi.Model;
public record Announcement
{
    public string Title { get; init; }
    public string Body { get; init; }

    public string? UrlRegex { get; init; } // List of page names? List<string>
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
}