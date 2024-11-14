namespace CO.CDP.EntityVerification.Model;

public record CountryIdentifiers
{
    /// <example>"5a360be7-e1d3-4214-9f72-0e1d6b57b85d"</example>
    public string? Id { get; init; }

    /// <example>"FR"</example>
    public required string CountryCode { get; init; }

    /// <example>"GB-PPON"</example>
    public required string Scheme { get; init; }
 
    /// <example>"Acme Corporation Ltd."</example>
    public required string RegisterName { get; init; }
}