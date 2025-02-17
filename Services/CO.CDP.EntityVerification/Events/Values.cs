namespace CO.CDP.EntityVerification.Events;

public class IdentifierSchemes
{
    public const string Ppon = "GB-PPON";
}

public record Identifier
{
    /// <example>"GB-PPON"</example>
    public required string Scheme { get; init; }

    /// <example>"5a360be7-e1d3-4214-9f72-0e1d6b57b85d"</example>
    public required string Id { get; init; }

    /// <example>"Acme Corporation Ltd."</example>
    public required string LegalName { get; init; }

    /// <example>"https://cdp.cabinetoffice.gov.uk/organisations/5a360be7-e1d3-4214-9f72-0e1d6b57b85d"</example>
    public Uri? Uri { get; init; }

    public static ICollection<CO.CDP.EntityVerification.Persistence.Identifier> GetPersistenceIdentifiers(
        IEnumerable<Identifier> evIds)
    {
        List<CO.CDP.EntityVerification.Persistence.Identifier> ids = [];

        foreach (var e in evIds)
        {
            ids.Add(new CO.CDP.EntityVerification.Persistence.Identifier
            {
                IdentifierId = e.Id,
                LegalName = e.LegalName,
                Scheme = e.Scheme,
                Uri = e.Uri
            });
        }

        return ids;
    }
}