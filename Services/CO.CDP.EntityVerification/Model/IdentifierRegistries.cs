namespace CO.CDP.EntityVerification.Model;

public record IdentifierRegistries
{
    /// <example>"FR"</example>
    public required string Countrycode { get; init; }

    /// <example>"FR-GDP"</example>
    public required string Scheme { get; init; }

    /// <example>"Acme Corporation Ltd."</example>
    public required string RegisterName { get; init; }
}