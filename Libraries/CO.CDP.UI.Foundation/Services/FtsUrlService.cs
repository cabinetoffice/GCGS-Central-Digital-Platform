using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Globalization;
using CO.CDP.UI.Foundation.Cookies;

namespace CO.CDP.UI.Foundation.Services;

/// <summary>
/// Configuration options for the FTS URL service
/// </summary>
public class FtsUrlOptions : ServiceUrlOptions
{
    public FtsUrlOptions()
    {
        SessionKey = "FtsServiceOrigin";
    }
}

/// <summary>
/// Interface for building URLs to the FTS service
/// </summary>
public interface IFtsUrlService
{
    /// <summary>
    /// Builds a URL to an FTS service endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint path</param>
    /// <param name="organisationId">Optional organisation ID</param>
    /// <param name="redirectUrl">Optional redirect URL</param>
    /// <returns>The complete URL to the FTS service endpoint</returns>
    string BuildUrl(string endpoint, Guid? organisationId = null, string? redirectUrl = null);
}

/// <summary>
/// Service for building URLs to the FTS service
/// </summary>
public class FtsUrlService : UrlServiceBase, IFtsUrlService
{
    /// <summary>
    /// Initialises a new instance of the FtsUrlService
    /// </summary>
    /// <param name="options">FTS URL options</param>
    /// <param name="httpContextAccessor">HTTP context accessor for session access</param>
    /// <param name="cookiePreferencesService">Optional cookie preferences service</param>
    public FtsUrlService(
        FtsUrlOptions options,
        IHttpContextAccessor httpContextAccessor,
        ICookiePreferencesService? cookiePreferencesService = null)
        : base(options, httpContextAccessor, cookiePreferencesService)
    {
    }
}