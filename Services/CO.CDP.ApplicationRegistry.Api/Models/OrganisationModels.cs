using CO.CDP.ApplicationRegistry.Core.Entities;

namespace CO.CDP.ApplicationRegistry.Api.Models;

/// <summary>
/// Request model for creating an organisation.
/// </summary>
public record CreateOrganisationRequest
{
    /// <summary>
    /// Gets or sets the CDP organisation GUID.
    /// </summary>
    public required Guid CdpOrganisationGuid { get; init; }

    /// <summary>
    /// Gets or sets the organisation name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets whether the organisation is active.
    /// </summary>
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// Request model for updating an organisation.
/// </summary>
public record UpdateOrganisationRequest
{
    /// <summary>
    /// Gets or sets the organisation name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets whether the organisation is active.
    /// </summary>
    public required bool IsActive { get; init; }
}

/// <summary>
/// Response model for an organisation.
/// </summary>
public record OrganisationResponse
{
    /// <summary>
    /// Gets or sets the organisation identifier.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets or sets the CDP organisation GUID.
    /// </summary>
    public required Guid CdpOrganisationGuid { get; init; }

    /// <summary>
    /// Gets or sets the organisation name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the organisation slug.
    /// </summary>
    public required string Slug { get; init; }

    /// <summary>
    /// Gets or sets whether the organisation is active.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Summary response model for an organisation (reduced details).
/// </summary>
public record OrganisationSummaryResponse
{
    /// <summary>
    /// Gets or sets the organisation identifier.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets or sets the CDP organisation GUID.
    /// </summary>
    public required Guid CdpOrganisationGuid { get; init; }

    /// <summary>
    /// Gets or sets the organisation name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the organisation slug.
    /// </summary>
    public required string Slug { get; init; }

    /// <summary>
    /// Gets or sets whether the organisation is active.
    /// </summary>
    public required bool IsActive { get; init; }
}

/// <summary>
/// Extension methods for organisation mapping.
/// </summary>
public static class OrganisationMappingExtensions
{
    public static OrganisationResponse ToResponse(this Organisation organisation)
    {
        return new OrganisationResponse
        {
            Id = organisation.Id,
            CdpOrganisationGuid = organisation.CdpOrganisationGuid,
            Name = organisation.Name,
            Slug = organisation.Slug,
            IsActive = organisation.IsActive,
            CreatedAt = organisation.CreatedAt
        };
    }

    public static OrganisationSummaryResponse ToSummaryResponse(this Organisation organisation)
    {
        return new OrganisationSummaryResponse
        {
            Id = organisation.Id,
            CdpOrganisationGuid = organisation.CdpOrganisationGuid,
            Name = organisation.Name,
            Slug = organisation.Slug,
            IsActive = organisation.IsActive
        };
    }
}
