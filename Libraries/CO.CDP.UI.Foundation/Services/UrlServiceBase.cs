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
    /// <param name="cookieAcceptance">Optional cookie acceptance override.</param>
    /// <returns>The complete URL to the service endpoint.</returns>
    public string BuildUrl(string endpoint, Guid? organisationId = null, string? redirectUrl = null, bool? cookieAcceptance = null)
    {
        var queryParams = new Dictionary<string, string?>();

        var culture = CultureInfo.CurrentUICulture.Name.Replace('-', '_');
        queryParams.Add("language", culture);

        if (organisationId.HasValue)
        {
            queryParams.Add("organisation_id", organisationId.Value.ToString());
        }

        if (!string.IsNullOrEmpty(redirectUrl))
        {
            queryParams.Add("redirect_url", redirectUrl);
        }

        if (cookieAcceptance.HasValue)
        {
            queryParams.Add("cookies_accepted", cookieAcceptance.Value.ToString().ToLower());
        }
        else if (_cookiePreferencesService != null)
        {
            var cookiesAccepted = _cookiePreferencesService.GetValue();
            string cookiesAcceptedValue = cookiesAccepted switch
            {
                CookieAcceptanceValues.Accept => "true",
                CookieAcceptanceValues.Reject => "false",
                _ => "unknown"
            };
            queryParams.Add("cookies_accepted", cookiesAcceptedValue);
        }

        return BuildUrl(endpoint, queryParams);
    }

    /// <summary>
    /// Builds a URL to a service endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint path.</param>
    /// <param name="queryParams">The query parameters.</param>
    /// <returns>The complete URL to the service endpoint.</returns>
    private string BuildUrl(string endpoint, Dictionary<string, string?> queryParams)
    {
        var baseUrl = GetBaseUrl();
        var url = new Uri(new Uri(baseUrl), endpoint).ToString();
        return QueryHelpers.AddQueryString(url, queryParams);
    }

    /// <summary>
    /// Returns the path for the given endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint path.</param>
    /// <returns>The path for the given endpoint.</returns>
    public string GetPath(string endpoint)
    {
        var uri = new Uri(new Uri(GetBaseUrl()), endpoint);
        return uri.AbsolutePath;
    }

    private string GetBaseUrl()
    {
        return _serviceBaseUrl;
    }
}