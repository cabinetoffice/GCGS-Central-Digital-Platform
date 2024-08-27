namespace CO.CDP.Organisation.WebApi.Events;

public record Identifier
{
    /// <example>"CDP-PPON"</example>
    public required string Scheme { get; init; }

    /// <example>"5a360be7-e1d3-4214-9f72-0e1d6b57b85d"</example>
    public string? Id { get; init; }

    /// <example>"Acme Corporation Ltd."</example>
    public required string LegalName { get; init; }

    /// <example>"https://cdp.cabinetoffice.gov.uk/organisations/5a360be7-e1d3-4214-9f72-0e1d6b57b85d"</example>
    public string? Uri { get; init; }
}

public record Address
{
    /// <example>"82 St. John’s Road"</example>
    public required string StreetAddress { get; init; }

    /// <example>"CHESTER"</example>
    public required string Locality { get; init; }

    /// <example>"Lancashire"</example>
    public string? Region { get; init; }

    /// <example>"CH43 7UR"</example>
    public required string PostalCode { get; init; }

    /// <example>"United Kingdom"</example>
    public required string CountryName { get; init; }

    /// <example>"GB"</example>
    public required string Country { get; init; }

    public required string Type { get; init; }
}

public record ContactPoint
{
    /// <example>"Procurement Team"</example>
    public string? Name { get; init; }

    /// <example>"procurement@example.com"</example>
    public string? Email { get; init; }

    /// <example>"079256123321"</example>
    public string? Telephone { get; init; }

    /// <example>"https://example.com"</example>
    public string? Url { get; init; }
}