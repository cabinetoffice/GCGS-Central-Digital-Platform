namespace CO.CDP.UserManagement.Core.Exceptions;

/// <summary>
/// Exception thrown when an invalid operation is attempted.
/// </summary>
public class InvalidOperationException : ApplicationRegistryException
{
    public InvalidOperationException(string message) : base(message)
    {
    }

    public InvalidOperationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
