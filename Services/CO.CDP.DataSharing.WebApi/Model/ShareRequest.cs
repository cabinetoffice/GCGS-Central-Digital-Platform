using System.ComponentModel.DataAnnotations;

namespace CO.CDP.DataSharing.WebApi.Model;

internal record ShareRequest
{
    /// <example>"3b3a269a-c1fa-4bfa-8892-7c6a9aef03bb"</example>
    [Required]
    public required Guid SupplierFormId { get; init; }

    /// <example>"2024-05-21T14:45:19.783Z"</example>
    [Required]
    public required DateTime ExpiresAt { get; init; }

    /// <example>[]</example>
    [Required]
    public required List<string> Permissions { get; init; } = new();
}