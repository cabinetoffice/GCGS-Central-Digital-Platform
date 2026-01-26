namespace CO.CDP.Authentication.Tests;

public record UserDetails
{
    public required string UserUrn { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public Guid? PersonId { get; init; }
}
