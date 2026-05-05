namespace CO.CDP.UserManagement.Infrastructure.Events;

public record PersonRemovedFromOrganisation
{
    public required string OrganisationId { get; init; }
    public required string PersonId { get; init; }
}