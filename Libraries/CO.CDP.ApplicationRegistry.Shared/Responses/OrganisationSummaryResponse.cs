namespace CO.CDP.ApplicationRegistry.Shared.Responses;

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
