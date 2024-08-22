namespace CO.CDP.Organisation.WebApi.Model;

public record PersonInviteModel
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public List<string> Scopes { get; init; } = [];
}