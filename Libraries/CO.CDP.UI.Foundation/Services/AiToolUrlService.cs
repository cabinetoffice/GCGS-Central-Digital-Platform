using CO.CDP.UI.Foundation.Cookies;
using Microsoft.AspNetCore.Http;

namespace CO.CDP.UI.Foundation.Services;

/// <summary>
/// Configuration options for the AI Tool URL service
/// </summary>
public class AiToolUrlOptions : ServiceUrlOptions
{
    public AiToolUrlOptions()
    {
        SessionKey = "AiToolServiceOrigin";
    }
}

/// <summary>
/// Service for building URLs to the AI Tool service
/// </summary>
public class AiToolUrlService : UrlServiceBase, IAiToolUrlService
{
    /// <summary>
    /// Initialises a new instance of the AiToolUrlService
    /// </summary>
    /// <param name="options">AI Tool URL options</param>
    /// <param name="httpContextAccessor">HTTP context accessor for session access</param>
    /// <param name="cookiePreferencesService">Optional cookie preferences service</param>
    public AiToolUrlService(
        AiToolUrlOptions options,
        IHttpContextAccessor httpContextAccessor,
        ICookiePreferencesService? cookiePreferencesService = null)
        : base(options, httpContextAccessor, cookiePreferencesService)
    {
    }

    /// <summary>
    /// Builds a URL to an AI Tool service endpoint with optional additional query parameters
    /// </summary>
    /// <param name="endpoint">The endpoint path</param>
    /// <param name="organisationId">Optional organisation ID</param>
    /// <param name="redirectUri">Optional redirect URI</param>
    /// <param name="cookieAcceptance">Optional cookie acceptance override</param>
    /// <param name="additionalParams">Additional query parameters to include</param>
    /// <returns>The complete URL to the AI Tool service endpoint</returns>
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
