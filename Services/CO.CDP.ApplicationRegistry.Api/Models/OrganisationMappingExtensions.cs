using CO.CDP.ApplicationRegistry.Shared.Responses;
using OrganisationEntity = CO.CDP.ApplicationRegistry.Core.Entities.Organisation;

namespace CO.CDP.ApplicationRegistry.Api.Models;

/// <summary>
/// Extension methods for organisation mapping.
/// </summary>
public static class OrganisationMappingExtensions
{
    public static OrganisationResponse ToResponse(this OrganisationEntity organisation)
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

    public static OrganisationSummaryResponse ToSummaryResponse(this OrganisationEntity organisation)
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