namespace CO.CDP.Organisation.WebApi.Model;
public record Announcement
{
    public string Title { get; init; }
    public string Body { get; init; }
    public List<string> Pages { get; init; }
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public List<string> Scopes { get; set; } = [];
}