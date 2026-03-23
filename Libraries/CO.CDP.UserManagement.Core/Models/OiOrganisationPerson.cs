namespace CO.CDP.UserManagement.Core.Models;

/// <summary>
/// Represents a person's identity and Organisation Information scopes as returned by the Organisation API.
/// Used for scope-vs-role comparison logging only; never drives access decisions for non-FTS/SIRSI.
/// </summary>
public record OiOrganisationPerson
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required IReadOnlyList<string> Scopes { get; init; }
}
