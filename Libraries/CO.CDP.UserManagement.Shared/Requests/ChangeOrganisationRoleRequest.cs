using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.Shared.Requests;

/// <summary>
/// Request model for changing an organisation role.
/// </summary>
public record ChangeOrganisationRoleRequest
{
    /// <summary>
    /// Gets or sets the organisation role.
    /// </summary>
    public OrganisationRole OrganisationRole { get; init; }
}
