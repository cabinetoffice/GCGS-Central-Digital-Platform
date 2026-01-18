using CO.CDP.ApplicationRegistry.Core.Entities;

namespace CO.CDP.ApplicationRegistry.Api.Models;

/// <summary>
/// Request model for enabling an application for an organisation.
/// </summary>
public record EnableApplicationRequest
{
    /// <summary>
    /// Gets or sets the application identifier to enable.
    /// </summary>
    public required int ApplicationId { get; init; }
}

/// <summary>
/// Response model for an organisation-application relationship.
/// </summary>
public record OrganisationApplicationResponse
{
    /// <summary>
    /// Gets or sets the organisation application identifier.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets or sets the organisation identifier.
    /// </summary>
    public required int OrganisationId { get; init; }

    /// <summary>
    /// Gets or sets the organisation details.
    /// </summary>
    public OrganisationSummaryResponse? Organisation { get; init; }

    /// <summary>
    /// Gets or sets the application identifier.
    /// </summary>
    public required int ApplicationId { get; init; }

    /// <summary>
    /// Gets or sets the application details.
    /// </summary>
    public ApplicationSummaryResponse? Application { get; init; }

    /// <summary>
    /// Gets or sets whether this application is active for the organisation.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Gets or sets the date and time when the application was enabled.
    /// </summary>
    public DateTimeOffset? EnabledAt { get; init; }

    /// <summary>
    /// Gets or sets the identifier of the user who enabled the application.
    /// </summary>
    public string? EnabledBy { get; init; }

    /// <summary>
    /// Gets or sets the date and time when the application was disabled.
    /// </summary>
    public DateTimeOffset? DisabledAt { get; init; }

    /// <summary>
    /// Gets or sets the identifier of the user who disabled the application.
    /// </summary>
    public string? DisabledBy { get; init; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }
}

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
