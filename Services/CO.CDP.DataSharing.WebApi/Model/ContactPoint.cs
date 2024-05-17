using System.ComponentModel.DataAnnotations;

namespace CO.CDP.DataSharing.WebApi.Model;

internal record ContactPoint
{
    /// <example>"Procurement Team"</example>
    [Required] public required string Name { get; init; }
    /// <example>"procurement@example.com"</example>
    public string? Email { get; init; }
    /// <example>"079256123321"</example>
    public string? Telephone { get; init; }
    /// <example>"02080776644"</example>
    public string? FaxNumber { get; init; }
    /// <example>"https://example.com"</example>
    public Uri? Url { get; init; }
}