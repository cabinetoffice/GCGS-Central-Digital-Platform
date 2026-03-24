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
/// Service for building URLs to the User Management service
/// </summary>
public class UserManagementUrlService : UrlServiceBase, IUserManagementUrlService
{
    /// <summary>
    /// Initialises a new instance of the UserManagementUrlService
    /// </summary>
    /// <param name="options">User Management URL options</param>
    /// <param name="httpContextAccessor">HTTP context accessor for session access</param>
    /// <param name="cookiePreferencesService">Optional cookie preferences service</param>
    public UserManagementUrlService(
        UserManagementUrlOptions options,
        IHttpContextAccessor httpContextAccessor,
        ICookiePreferencesService? cookiePreferencesService = null)
        : base(options, httpContextAccessor, cookiePreferencesService)
    {
    }

    /// <summary>
    /// Builds a URL to a User Management service endpoint with optional additional query parameters
    /// </summary>
    /// <param name="endpoint">The endpoint path</param>
    /// <param name="organisationId">Optional organisation ID</param>
    /// <param name="redirectUri">Optional redirect URI</param>
    /// <param name="cookieAcceptance">Optional cookie acceptance override</param>
    /// <param name="additionalParams">Additional query parameters to include</param>
    /// <returns>The complete URL to the User Management service endpoint</returns>
    public string BuildUrl(string endpoint, Guid? organisationId = null, string? redirectUri = null, bool? cookieAcceptance = null, Dictionary<string, string?>? additionalParams = null)
    {
        var url = base.BuildUrl(endpoint, organisationId, redirectUri, cookieAcceptance);

        if (additionalParams != null && additionalParams.Any())
        {
            return Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(url, additionalParams);
        }

        return url;
    }
}

