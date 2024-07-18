using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.EntityVerification.Persistence;

[Index(nameof(PponId), IsUnique = true)]
public class Ppon : IEntityDate
{
    public int Id { get; set; }
    public required string PponId { get; set; }
    public int IdentifierId { get; set; }
    public required Identifier Identifier { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}