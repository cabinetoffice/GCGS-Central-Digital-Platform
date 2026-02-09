namespace CO.CDP.ApplicationRegistry.Shared.Enums;

/// <summary>
/// Defines the roles a user can have within an organisation.
/// </summary>
public enum OrganisationRole
{
    /// <summary>
    /// Standard member with basic access.
    /// </summary>
    Member = 0,

    /// <summary>
    /// Administrator with elevated permissions.
    /// </summary>
    Admin = 1,

    /// <summary>
    /// Owner with full control over the organisation.
    /// </summary>
    Owner = 2
}
