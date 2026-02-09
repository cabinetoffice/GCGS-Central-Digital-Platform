namespace CO.CDP.UserManagement.Core.Exceptions;

/// <summary>
/// Base exception for all application registry related exceptions.
/// </summary>
public class ApplicationRegistryException : Exception
{
    public ApplicationRegistryException(string message) : base(message)
    {
    }

    public ApplicationRegistryException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
