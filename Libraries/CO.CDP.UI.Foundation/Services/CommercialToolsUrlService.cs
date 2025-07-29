using CO.CDP.UI.Foundation.Cookies;
using Microsoft.AspNetCore.Http;

namespace CO.CDP.UI.Foundation.Services;

/// <summary>
/// Configuration options for the Commercial Tools URL service
/// </summary>
public class CommercialToolsUrlOptions : ServiceUrlOptions
{
    public CommercialToolsUrlOptions()
    {
        SessionKey = "CommercialToolsServiceOrigin";
    }
}

/// <summary>
/// Service for building URLs to the Commercial Tools service
/// </summary>
public class CommercialToolsUrlService : UrlServiceBase, ICommercialToolsUrlService
{
    /// <summary>
    /// Initialises a new instance of the CommercialToolsUrlService
    /// </summary>
    /// <param name="options">Commercial Tools URL options</param>
    /// <param name="httpContextAccessor">HTTP context accessor for session access</param>
    /// <param name="cookiePreferencesService">Optional cookie preferences service</param>
    public CommercialToolsUrlService(
        CommercialToolsUrlOptions options,
        IHttpContextAccessor httpContextAccessor,
        ICookiePreferencesService? cookiePreferencesService = null)
        : base(options, httpContextAccessor, cookiePreferencesService)
    {
    }

    /// <summary>
    /// Builds a URL to a Commercial Tools service endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint path</param>
    /// <param name="organisationId">Optional organisation ID</param>
    /// <param name="redirectUrl">Optional redirect URL</param>
    /// <param name="cookieAcceptance">Optional cookie acceptance override</param>
    /// <returns>The complete URL to the Commercial Tools service endpoint</returns>
    public new string BuildUrl(string endpoint, Guid? organisationId = null, string? redirectUrl = null, bool? cookieAcceptance = null)
    {
        return base.BuildUrl(endpoint, organisationId, redirectUrl, cookieAcceptance);
    }
}
