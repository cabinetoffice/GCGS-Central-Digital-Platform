namespace CO.CDP.UserManagement.Core.Exceptions;

/// <summary>
/// Thrown when the current user is not permitted to perform a membership operation
/// (e.g. self-removal, an Admin attempting to modify an Owner).
/// Maps to HTTP 403 Forbidden.
/// </summary>
public class MembershipOperationForbiddenException(string message) : UserManagementException(message);
