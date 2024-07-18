using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.EntityVerification.Persistence;

[Index(nameof(PponId), IsUnique = true)]
public class Ppon : IEntityDate
{
    public int Id { get; set; }
    public required string PponId { get; set; }
    public required Guid OrganisationId { get; set; }
    public ICollection<Identifier> Identifiers { get; set; } = [];
    public required string Name { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}
