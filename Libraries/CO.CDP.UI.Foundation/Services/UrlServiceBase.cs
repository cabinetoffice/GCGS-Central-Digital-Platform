using CO.CDP.UI.Foundation.Cookies;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;

namespace CO.CDP.UI.Foundation.Services;

/// <summary>
/// Configuration options for a URL service.
/// </summary>
public class ServiceUrlOptions
{
    /// <summary>
    /// The base URL of the service.
    /// </summary>
    public string? ServiceBaseUrl { get; set; }

    /// <summary>
    /// The session key used to store the service origin.
    /// </summary>
    public required string SessionKey { get; set; }
}

/// <summary>
/// Base class for building URLs to an external service.
/// </summary>
public abstract class UrlServiceBase
{
    private readonly string _serviceBaseUrl;
    private readonly ICookiePreferencesService? _cookiePreferencesService;

    /// <summary>
    /// Initialises a new instance of the <see cref="UrlServiceBase"/>.
    /// </summary>
    protected UrlServiceBase(
        ServiceUrlOptions options,
        IHttpContextAccessor httpContextAccessor,
        ICookiePreferencesService? cookiePreferencesService = null)
    {
        _cookiePreferencesService = cookiePreferencesService;

        var session = httpContextAccessor.HttpContext?.Session;
        var serviceBaseUrl = session?.GetString(options.SessionKey)
                             ?? options.ServiceBaseUrl
                             ?? throw new InvalidOperationException("Service base URL is not configured.");

        _serviceBaseUrl = serviceBaseUrl.TrimEnd('/');
    }

    /// <summary>
    /// Builds a URL to a service endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint path.</param>
    /// <param name="organisationId">Optional organisation ID.</param>
    /// <param name="redirectUrl">Optional redirect URL.</param>
    /// <returns>The complete URL to the service endpoint.</returns>
    public string BuildUrl(string endpoint, Guid? organisationId = null, string? redirectUrl = null)
    {
        var uriBuilder = new UriBuilder(_serviceBaseUrl)
        {
            Path = $"{endpoint.TrimStart('/')}"
        };

        var queryBuilder = new QueryBuilder
        {
            { "language", CultureInfo.CurrentUICulture.Name.Replace("-", "_") }
        };

        if (organisationId.HasValue)
        {
            queryBuilder.Add("organisation_id", organisationId.Value.ToString());
        }

        if (!string.IsNullOrEmpty(redirectUrl))
        {
            queryBuilder.Add("redirect_url", redirectUrl);
        }

        if (_cookiePreferencesService != null)
        {
            var cookiesAccepted = _cookiePreferencesService.IsAccepted() ? "true" : "false";
            queryBuilder.Add("cookies_accepted", cookiesAccepted);
        }

        uriBuilder.Query = queryBuilder.ToString();
        return uriBuilder.Uri.ToString();
    }
}