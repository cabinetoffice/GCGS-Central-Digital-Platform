namespace CO.CDP.ApplicationRegistry.Shared.Requests;

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
