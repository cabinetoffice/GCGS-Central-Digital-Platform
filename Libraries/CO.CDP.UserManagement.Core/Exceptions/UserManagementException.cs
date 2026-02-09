namespace CO.CDP.UserManagement.Core.Exceptions;

/// <summary>
/// Base exception for all user management related exceptions.
/// </summary>
public class UserManagementException : Exception
{
    protected UserManagementException(string message) : base(message)
    {
    }

    protected UserManagementException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
