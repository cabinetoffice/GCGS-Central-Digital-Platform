namespace CO.CDP.Organisation.WebApi.Model;
public record Announcement
{
    public required string Title { get; init; }
    public required string Body { get; init; }

    public string? UrlRegex { get; init; } // List of page names? List<string>
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
}