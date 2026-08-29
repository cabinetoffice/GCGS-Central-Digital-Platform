namespace CO.CDP.UI.Foundation.Services;

/// <summary>
/// Interface for building URLs to the User Management service
/// </summary>
public interface IUserManagementUrlService
{
    /// <summary>
    /// Builds a URL to the User Management service for the given organisation
    /// </summary>
    /// <param name="organisationId">The organisation ID to include in the URL path</param>
    /// <returns>The complete URL to the User Management service</returns>
    string BuildOrganisationUrl(Guid organisationId);
}
