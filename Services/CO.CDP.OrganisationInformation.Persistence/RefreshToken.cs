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
    /// <summary>
    /// The authenticated user's URN stored separately from the token string.
    /// Never included in the opaque token value — the token string carries only
    /// the random password and random salt (RFC 6749 §10.10 opacity requirement).
    /// </summary>
    public string? UserUrn { get; set; }
    /// <summary>
    /// Base64-encoded random salt used for PBKDF2 hashing.
    /// Stored so that ValidateAndRevokeRefreshToken can re-derive the hash.
    /// </summary>
    public string? Salt { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}