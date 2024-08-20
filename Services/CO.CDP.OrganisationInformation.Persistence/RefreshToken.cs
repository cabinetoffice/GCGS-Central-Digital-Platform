using CO.CDP.EntityFrameworkCore.Timestamps;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

[Index(nameof(TokenHash), IsUnique = true)]
public class RefreshToken : IEntityDate
{
    public int Id { get; set; }
    public required string TokenHash { get; set; }
    public required DateTime ExpiryDate { get; set; }
    public bool? Revoked { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}