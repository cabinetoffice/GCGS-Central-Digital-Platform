using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationInformation;

/// <summary>
/// Based on OCDS <a href="https://standard.open-contracting.org/latest/en/schema/reference/#contactpoint">ContactPoint</a>.
/// </summary>
public record ContactPoint
{
    /// <example>"Procurement Team"</example>
    public required string Name { get; init; }

    /// <example>"procurement@example.com"</example>
    public string? Email { get; init; }

    /// <example>"079256123321"</example>
    public string? Telephone { get; init; }

    /// <example>"02080776644"</example>
    public string? FaxNumber { get; init; }

    /// <example>"https://example.com"</example>
    public Uri? Url { get; init; }
}