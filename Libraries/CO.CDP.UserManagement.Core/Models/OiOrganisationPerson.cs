namespace CO.CDP.UserManagement.Core.Models;

/// <summary>
/// Represents a person's identity and Organisation Information scopes as returned by the Organisation API.
/// Used for person enrichment and scope-vs-role comparison logging.
/// </summary>
public record OiOrganisationPerson
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required IReadOnlyList<string> Scopes { get; init; }
}
