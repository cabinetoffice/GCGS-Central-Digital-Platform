using System.ComponentModel.DataAnnotations;

namespace CO.CDP.DataSharing.WebApi.Model;

internal record Identifier
{
    [Required(AllowEmptyStrings = true)] public required string Scheme { get; init; }
    [Required] public required Guid Id { get; init; }
    public string? LegalName { get; init; }
    public string? Uri { get; init; }
}