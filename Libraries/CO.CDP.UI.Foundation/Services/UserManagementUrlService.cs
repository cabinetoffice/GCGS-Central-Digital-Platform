using CO.CDP.UI.Foundation.Cookies;
using Microsoft.AspNetCore.Http;

namespace CO.CDP.UI.Foundation.Services;

/// <summary>
/// Configuration options for the User Management URL service
/// </summary>
public class UserManagementUrlOptions : ServiceUrlOptions
{
    public UserManagementUrlOptions()
    {
        SessionKey = "UserManagementServiceOrigin";
    }
}

/// <summary>
/// Service for building URLs to the User Management service.
/// Puts the organisation ID in the URL path (not as a query parameter)
/// to match UserManagement.App's routing convention: /organisation/{id}
/// </summary>
public class UserManagementUrlService(
    UserManagementUrlOptions options,
    IHttpContextAccessor httpContextAccessor,
    ICookiePreferencesService? cookiePreferencesService = null)
    : UrlServiceBase(options, httpContextAccessor, cookiePreferencesService), IUserManagementUrlService
{
    /// <summary>
    /// Builds a URL to the User Management service for the given organisation.
    /// The organisation ID is embedded in the path: /organisation/{id}
    /// </summary>
    /// <param name="organisationId">The organisation ID to include in the URL path</param>
    /// <returns>The complete URL to the User Management service</returns>
    public string BuildOrganisationUrl(Guid organisationId)
    {
        return BuildUrl($"organisation/{organisationId}", new Dictionary<string, string?>());
    }
}
