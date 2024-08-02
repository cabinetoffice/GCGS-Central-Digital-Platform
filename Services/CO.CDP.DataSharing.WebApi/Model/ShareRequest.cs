using System.ComponentModel.DataAnnotations;

namespace CO.CDP.DataSharing.WebApi.Model;

public record ShareRequest
{
    /// <example>"3032d31a-c3d9-45a7-a271-cc4f24d2fc03"</example>
    [Required]
    public required Guid FormId { get; init; }

    /// <example>"3b3a269a-c1fa-4bfa-8892-7c6a9aef03bb"</example>
    [Required]
    public required Guid OrganisationId { get; init; }
}