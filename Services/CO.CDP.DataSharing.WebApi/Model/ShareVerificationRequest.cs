using System.ComponentModel.DataAnnotations;

namespace CO.CDP.DataSharing.WebApi.Model;

public record ShareVerificationRequest
{
    /// <example>"f53dee53"</example>
    [Required]
    public required string ShareCode { get; init; }

    /// <example>"20240429"</example>
    public required string FormVersionId { get; init; }
}