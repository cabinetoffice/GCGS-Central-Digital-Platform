using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.EntityVerification.Persistence;

[Index(nameof(Id), IsUnique = true)]
public class Identifier : IEntityDate
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Scheme { get; set; }
    public Ppon Ppon { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}