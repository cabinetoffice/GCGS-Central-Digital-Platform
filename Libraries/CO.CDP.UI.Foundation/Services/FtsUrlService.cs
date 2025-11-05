using Microsoft.AspNetCore.Http;
using CO.CDP.UI.Foundation.Cookies;

namespace CO.CDP.UI.Foundation.Services;

/// <summary>
/// Configuration options for the FTS URL service.
/// </summary>
public class FtsUrlOptions : ServiceUrlOptions
{
    public FtsUrlOptions()
    {
        SessionKey = "FtsServiceOrigin";
    }
}


/// <summary>
/// Service for building URLs to the FTS service.
/// </summary>
public class FtsUrlService : UrlServiceBase, IFtsUrlService
{
    /// <summary>
    /// Initialises a new instance of the <see cref="FtsUrlService"/>.
    /// </summary>
    /// <param name="options">FTS URL options.</param>
    /// <param name="httpContextAccessor">HTTP context accessor for session access.</param>
    /// <param name="cookiePreferencesService">Optional cookie preferences service.</param>
    public FtsUrlService(
        FtsUrlOptions options,
        IHttpContextAccessor httpContextAccessor,
        ICookiePreferencesService? cookiePreferencesService = null)
        : base(options, httpContextAccessor, cookiePreferencesService)
    {
    }

    public new string BuildUrl(string endpoint, Guid? organisationId = null, string? redirectUri = null, bool? cookieAcceptance = null, string? redirectUriParamName = null)
    {
        return base.BuildUrl(endpoint, organisationId, redirectUri, cookieAcceptance, redirectUriParamName ?? "redirect_url");
    }
}