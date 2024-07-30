namespace CO.CDP.EntityVerification.Events;

public record PponGenerated : IEvent
{
    public required Guid OrganisationId { get; init; }

    /// <example>"d2dab085ec23481cb970ee6b372f9f57"</example>
    public required string Id { get; init; }

    /// <example>"GB-COH"</example>
    public required string Scheme { get; init; }

    /// <example>"Acme Ltd"</example>
    public required string LegalName { get; init; }

    /// <example>"588be0d7-7a61-4ed4-bcc6-f1bd9b1533cf"</example>
    public required Guid OrganisationId { get; init; }
}