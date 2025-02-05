using CO.CDP.EntityFrameworkCore.Timestamps;

namespace CO.CDP.EntityVerification.Persistence;

public class IdentifierRegistries : IEntityDate
{
    public int Id { get; set; }
    public required string CountryCode { get; set; }
    public required string Scheme { get; set; }
    public required string RegisterName { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }

}