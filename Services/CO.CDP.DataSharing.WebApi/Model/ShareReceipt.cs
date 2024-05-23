using System.ComponentModel.DataAnnotations;

namespace CO.CDP.DataSharing.WebApi.Model;

internal record ShareReceipt
{
    /// <example>"3b3a269a-c1fa-4bfa-8892-7c6a9aef03bb"</example>
    [Required]
    public required Guid FormId { get; init; }

    /// <example>"20240429"</example>
    [Required]
    public required string FormVersionId { get; init; }

    /// <example>"2024-05-21T14:45:19.783Z"</example>
    [Required]
    public required DateTime ExpiresAt { get; init; }

    /// <example>[]</example>
    [Required]
    public required List<string> Permissions { get; init; } = new();

    /// <example>"f53dee53-cf59-4725-9285-44f8960507b9"</example>
    [Required]
    public required string ShareCode { get; init; }
}