namespace CO.CDP.OrganisationInformation;

/// <summary>
/// Based on OCDS <a href="https://standard.open-contracting.org/latest/en/schema/reference/#address">Address</a>.
/// </summary>
public record Address
{
    /// <example>"82 St. John’s Road"</example>
    public required string StreetAddress { get; init; }

    /// <example>"Green Tower"</example>
    public string? StreetAddress2 { get; init; }

    /// <example>"CHESTER"</example>
    public required string Locality { get; init; }

    /// <example>"Lancashire"</example>
    public string? Region { get; init; }

    /// <example>"CH43 7UR"</example>
    public required string PostalCode { get; init; }

    /// <example>"United Kingdom"</example>
    public required string CountryName { get; init; }

    public required AddressType Type { get; init; }
}