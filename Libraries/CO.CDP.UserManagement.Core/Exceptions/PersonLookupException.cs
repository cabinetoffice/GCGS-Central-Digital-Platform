namespace CO.CDP.UserManagement.Core.Exceptions;

/// <summary>
/// Exception thrown when an error occurs during person lookup from the CDP Person service.
/// </summary>
public class PersonLookupException : ApplicationRegistryException
{
    public PersonLookupException(string message) : base(message)
    {
    }

    public PersonLookupException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
