namespace CO.CDP.UserManagement.Shared.Enums;

/// <summary>
/// Represents the status of a user within the organisation.
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// User invitation is pending acceptance.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// User is active in the organisation.
    /// </summary>
    Active = 1
}
