namespace CO.CDP.Organisation.WebApi.Events;

public record PersonRemovedFromOrganisation
{
    public required string OrganisationId { get; init; }
    public required string PersonId { get; init; }
}