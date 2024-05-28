using System.ComponentModel.DataAnnotations;

namespace CO.CDP.DataSharing.WebApi.Model;

internal record ShareVerificationReceipt
{
    /// <example>"20240429"</example>
    [Required]
    public required string FormVersionId { get; init; }

    /// <example>"f53dee53-cf59-4725-9285-44f8960507b9"</example>
    [Required]
    public required string ShareCode { get; init; }

    /// <example>true</example>
    [Required]
    public required bool IsLatest { get; init; }
}