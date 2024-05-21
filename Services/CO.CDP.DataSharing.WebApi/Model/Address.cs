using System.ComponentModel.DataAnnotations;

namespace CO.CDP.DataSharing.WebApi.Model;

/// <summary>
/// Based on OCDS <a href="https://standard.open-contracting.org/latest/en/schema/reference/#address">Address</a>.
/// </summary>
internal record Address
{
    /// <example>"82 St. John’s Road"</example>
    [Required] public required string StreetAddress { get; init; }
    /// <example>"CHESTER"</example>
    [Required] public required string Locality { get; init; }
    /// <example>"Lancashire"</example>
    [Required] public required string Region { get; init; }
    /// <example>"CH43 7UR"</example>
    [Required] public required string PostalCode { get; init; }
    /// <example>"United Kingdom"</example>
    [Required] public required string CountryName { get; init; }
}