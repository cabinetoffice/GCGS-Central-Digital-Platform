namespace CO.CDP.UserManagement.Shared.Requests;

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
