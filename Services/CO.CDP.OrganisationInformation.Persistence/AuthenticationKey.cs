using System.ComponentModel.DataAnnotations.Schema;
using CO.CDP.EntityFrameworkCore.Timestamps;

namespace CO.CDP.OrganisationInformation.Persistence;

public class AuthenticationKey : IEntityDate
{
    public int Id { get; set; }
    public required string Name { get; set; }
    /// <summary>SHA-256 (lowercase hex) of the API key. The plaintext key is never stored.</summary>
    public required string KeyHash { get; set; }
    /// <summary>Last characters of the key, to help users recognise it.</summary>
    public string? KeyHint { get; set; }

    [ForeignKey(nameof(Organisation))]
    public int? OrganisationId { get; set; }
    public bool Revoked { get; set; }
    public Organisation? Organisation { get; set; }
    public List<string> Scopes { get; set; } = [];
    public DateTimeOffset? RevokedOn { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}