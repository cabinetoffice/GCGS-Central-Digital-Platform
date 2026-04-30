namespace CO.CDP.Organisation.WebApi.Events;

public record PersonScopesUpdated
{
    public required string OrganisationId { get; init; }
    public required string PersonId { get; init; }
    public required List<string> Scopes { get; init; }
}