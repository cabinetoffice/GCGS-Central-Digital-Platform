namespace CO.CDP.UserManagement.Core.Exceptions;

/// <summary>
/// Thrown when an attempt is made to remove the last active Owner from an organisation.
/// </summary>
public class LastOwnerRemovalException : UserManagementException
{
    public LastOwnerRemovalException(Guid organisationId)
        : base($"Cannot remove the last Owner from organisation {organisationId}.")
    {
    }
}
