using CO.CDP.ApplicationRegistry.Core.Entities;

namespace CO.CDP.ApplicationRegistry.Api.Models;

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