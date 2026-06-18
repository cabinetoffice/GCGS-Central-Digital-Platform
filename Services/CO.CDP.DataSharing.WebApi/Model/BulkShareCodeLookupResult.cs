using CO.CDP.OrganisationInformation;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.DataSharing.WebApi.Model;

public record BulkShareCodeLookupResult
{
    /// <summary>The share code that was looked up.</summary>
    /// <example>"ABC12345"</example>
    [Required]
    public required string ShareCode { get; init; }

    /// <summary>Indicates whether the share code exists and has been submitted.</summary>
    /// <example>true</example>
    [Required]
    public required bool IsValid { get; init; }

    /// <summary>Organisation identifier, present when the share code is valid.</summary>
    /// <example>"47e6a363-11c0-4cf4-bce6-dea03034e4bb"</example>
    public Guid? OrganisationId { get; init; }

    /// <summary>Organisation name, present when the share code is valid.</summary>
    /// <example>"Acme Corporation"</example>
    public string? OrganisationName { get; init; }

    /// <summary>When the share code was submitted, present when the share code is valid.</summary>
    public DateTimeOffset? SubmittedAt { get; init; }

    [Required] public required OrganisationInformation.Identifier Identifier { get; init; }
    [Required] public required List<OrganisationInformation.Identifier> AdditionalIdentifiers { get; init; } = [];
    [Required] public required Address Address { get; init; }
    [Required] public required List<Address> AdditionalAddresses { get; init; } = [];
    [Required] public required ContactPoint ContactPoint { get; init; }
}
