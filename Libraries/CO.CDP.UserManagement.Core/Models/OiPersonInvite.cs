namespace CO.CDP.UserManagement.Core.Models;

/// <summary>
/// Represents a pending person invite as returned by the Organisation API.
/// Carries identity details (name, email, expiry) needed to enrich a UM <c>InviteRoleMapping</c>.
/// </summary>
public record OiPersonInvite
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public DateTimeOffset? ExpiresOn { get; init; }
    public DateTimeOffset? CreatedOn { get; init; }
}
