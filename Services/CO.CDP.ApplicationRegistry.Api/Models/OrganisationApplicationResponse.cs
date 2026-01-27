namespace CO.CDP.ApplicationRegistry.Api.Models;

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