namespace CO.CDP.UserManagement.Api.Events;

public record OrganisationUpdated
{
    public required string Id { get; init; }
    public required string Name { get; init; }
}