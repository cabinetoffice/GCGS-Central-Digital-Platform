namespace CO.CDP.EntityVerification.Model;

public record CountryIdentifiers
{
    /// <example>"FR"</example>
    public required string CountryCode { get; init; }

    /// <example>"GB-PPON"</example>
    public required string Scheme { get; init; }
 
    /// <example>"Acme Corporation Ltd."</example>
    public required string RegisterName { get; init; }
}