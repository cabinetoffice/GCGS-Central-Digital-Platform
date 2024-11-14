using CO.CDP.EntityFrameworkCore.Timestamps;

namespace CO.CDP.EntityVerification.Persistence;

public class Ppon : IEntityDate
{
    public int Id { get; set; }
    public required string IdentifierId { get; set; }
    public required Guid OrganisationId { get; set; }
    public ICollection<Identifier> Identifiers { get; set; } = [];
    public required string Name { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
    public DateTimeOffset startsOn { get; set; }
    public DateTimeOffset? endsOn { get; set; }

}