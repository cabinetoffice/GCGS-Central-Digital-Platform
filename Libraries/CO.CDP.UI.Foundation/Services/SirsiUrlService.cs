using CO.CDP.UI.Foundation.Cookies;
using Microsoft.AspNetCore.Http;

namespace CO.CDP.UI.Foundation.Services;

/// <summary>
/// Configuration options for the Sirsi URL service.
/// </summary>
public class SirsiUrlOptions : ServiceUrlOptions
{
    public SirsiUrlOptions()
    {
        SessionKey = "SirsiServiceOrigin";
    }
}


/// <summary>
/// Service for building URLs to the Sirsi service.
/// </summary>
public class SirsiUrlService : UrlServiceBase, ISirsiUrlService
{
    /// <summary>
    /// Initialises a new instance of the <see cref="SirsiUrlService"/>.
    /// </summary>
    /// <param name="options">Sirsi URL options.</param>
    /// <param name="httpContextAccessor">HTTP context accessor for session access.</param>
    /// <param name="cookiePreferencesService">Optional cookie preferences service.</param>
    public SirsiUrlService(
        SirsiUrlOptions options,
        IHttpContextAccessor httpContextAccessor,
        ICookiePreferencesService? cookiePreferencesService = null)
        : base(options, httpContextAccessor, cookiePreferencesService)
    {
    }

    public new string BuildUrl(string endpoint, Guid? organisationId = null, string? redirectUrl = null, bool? cookieAcceptance = null)
    {
        return base.BuildUrl(endpoint, organisationId, redirectUrl, cookieAcceptance);
    }
}
