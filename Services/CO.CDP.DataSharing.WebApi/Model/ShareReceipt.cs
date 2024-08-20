using System.ComponentModel.DataAnnotations;

namespace CO.CDP.DataSharing.WebApi.Model;

public record ShareReceipt
{
    /// <example>"3b3a269a-c1fa-4bfa-8892-7c6a9aef03bb"</example>
    [Required]
    public required Guid FormId { get; init; }

    /// <example>"20240429"</example>
    public string? FormVersionId { get; init; }

    /// <example>"f53dee53-cf59-4725-9285-44f8960507b9"</example>
    [Required]
    public required string ShareCode { get; init; }
}