using CO.CDP.ApplicationRegistry.Core.Entities;

namespace CO.CDP.ApplicationRegistry.Api.Models;

/// <summary>
/// Extension methods for organisation-application mapping.
/// </summary>
public static class OrganisationApplicationMappingExtensions
{
    /// <summary>
    /// Converts an OrganisationApplication entity to an OrganisationApplicationResponse.
    /// </summary>
    /// <param name="orgApp">The organisation application entity.</param>
    /// <param name="includeDetails">Whether to include organisation and application details.</param>
    /// <returns>The organisation application response model.</returns>
    public static OrganisationApplicationResponse ToResponse(
        this OrganisationApplication orgApp,
        bool includeDetails = true)
    {
        return new OrganisationApplicationResponse
        {
            Id = orgApp.Id,
            OrganisationId = orgApp.OrganisationId,
            Organisation = includeDetails && orgApp.Organisation != null
                ? orgApp.Organisation.ToSummaryResponse()
                : null,
            ApplicationId = orgApp.ApplicationId,
            Application = includeDetails && orgApp.Application != null
                ? orgApp.Application.ToSummaryResponse()
                : null,
            IsActive = orgApp.IsActive,
            EnabledAt = orgApp.EnabledAt,
            EnabledBy = orgApp.EnabledBy,
            DisabledAt = orgApp.DisabledAt,
            DisabledBy = orgApp.DisabledBy,
            CreatedAt = orgApp.CreatedAt
        };
    }

    /// <summary>
    /// Converts a collection of OrganisationApplication entities to OrganisationApplicationResponse models.
    /// </summary>
    /// <param name="orgApps">The collection of organisation application entities.</param>
    /// <param name="includeDetails">Whether to include organisation and application details.</param>
    /// <returns>The collection of organisation application response models.</returns>
    public static IEnumerable<OrganisationApplicationResponse> ToResponses(
        this IEnumerable<OrganisationApplication> orgApps,
        bool includeDetails = true)
    {
        return orgApps.Select(oa => oa.ToResponse(includeDetails));
    }
}