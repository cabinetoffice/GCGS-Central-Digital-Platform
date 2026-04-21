namespace CO.CDP.UserManagement.Api.Events;

public record OrganisationRegistered
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required List<string> Roles { get; init; }
    public required int Type { get; init; }
    public Guid? FounderPersonId { get; init; }
    public string? FounderUserUrn { get; init; }
}