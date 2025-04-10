using CO.CDP.EntityFrameworkCore.Timestamps;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

[Index(nameof(Guid), IsUnique = true)]
public class Announcement : IEntityDate
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }
    public required string Title { get; init; }
    public required string Body { get; init; }

    public List<string>? Pages { get; init; }
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public List<string> Scopes { get; set; } = [];
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}