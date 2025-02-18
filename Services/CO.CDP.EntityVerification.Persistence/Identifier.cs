using CO.CDP.EntityFrameworkCore.Timestamps;

namespace CO.CDP.EntityVerification.Persistence;

public class Identifier : IEntityDate
{
    public int Id { get; set; }
    public string? IdentifierId { get; set; }
    public required string Scheme { get; set; }
    public required string LegalName { get; set; }
    public Uri? Uri { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
    public DateTimeOffset startsOn { get; set; }
    public DateTimeOffset? endsOn { get; set; }
}