namespace CO.CDP.UserManagement.Shared.Enums;

/// <summary>
/// Defines the roles a user can have within an organisation.
/// </summary>
public enum OrganisationRole
{
    /// <summary>
    /// External agent associated to the organisation for application access.
    /// </summary>
    Agent = 0,

    /// <summary>
    /// Standard member with basic access.
    /// </summary>
    Member = 1,

    /// <summary>
    /// Administrator with elevated permissions.
    /// </summary>
    Admin = 2,

    /// <summary>
    /// Owner with full control over the organisation.
    /// </summary>
    Owner = 3
}
