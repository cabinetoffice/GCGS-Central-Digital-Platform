using System.ComponentModel.DataAnnotations;

namespace CO.CDP.DataSharing.WebApi.Model;

public record BulkShareCodeLookupRequest
{
    /// <summary>List of share codes to look up.</summary>
    /// <example>["ABC12345", "XYZ67890"]</example>
    [Required, MinLength(1)]
    public required IEnumerable<string> ShareCodes { get; init; }
}
