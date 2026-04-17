namespace CO.CDP.UserManagement.Core.Models;

/// <summary>
/// Represents an organisation join request as returned by the Organisation API.
/// </summary>
public sealed record OiJoinRequest
{
    public required Guid Id { get; init; }
    public required Guid PersonId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string Status { get; init; }
}