using CO.CDP.EntityFrameworkCore.Timestamps;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

[Index(nameof(TokenHash), IsUnique = true)]
public class RefreshToken : IEntityDate
{
    public int Id { get; set; }
    public required string TokenHash { get; set; }
    public required DateTimeOffset ExpiryDate { get; set; }
    public bool? Revoked { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}