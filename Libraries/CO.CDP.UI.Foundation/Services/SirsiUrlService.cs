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
/// Interface for building URLs to the Sirsi service.
/// </summary>
public interface ISirsiUrlService
{
    /// <summary>
    /// Builds a URL to a Sirsi service endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint path.</param>
    /// <param name="organisationId">Optional organisation ID.</param>
    /// <param name="redirectUrl">Optional redirect URL.</param>
    /// <returns>The complete URL to the Sirsi service endpoint.</returns>
    string BuildUrl(string endpoint, Guid? organisationId = null, string? redirectUrl = null);
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
}
