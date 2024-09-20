using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationInformation;

/// <summary>
/// Based on OCDS <a href="https://standard.open-contracting.org/latest/en/schema/reference/#identifier">Identifier</a>.
/// </summary>
public record Identifier
{
    /// <example>"GB-PPON"</example>
    [Required]
    public required string Scheme { get; init; }

    /// <example>"5a360be7-e1d3-4214-9f72-0e1d6b57b85d"</example>
    public string? Id { get; init; }

    /// <example>"Acme Corporation Ltd."</example>
    [Required(AllowEmptyStrings = true)]
    public required string LegalName { get; init; }

    /// <example>"https://cdp.cabinetoffice.gov.uk/organisations/5a360be7-e1d3-4214-9f72-0e1d6b57b85d"</example>
    public Uri? Uri { get; init; }
}