namespace CO.CDP.Organisation.WebApi.Events;

public class PponGenerated
{
    /// <example>"5cfc9e66-a588-49fa-937b-ce4ec0b9d093"</example>
    public required Guid OrganisationId { get; init; }

    /// <example>"d2dab085ec23481cb970ee6b372f9f57"</example>
    public required string Id { get; init; }

    /// <example>"CDP-PPON"</example>
    public required string Scheme { get; init; }

    /// <example>"Acme Ltd"</example>
    public required string LegalName { get; init; }
}