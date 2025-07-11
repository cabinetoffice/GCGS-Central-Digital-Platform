using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Globalization;
using CO.CDP.UI.Foundation.Cookies;

namespace CO.CDP.UI.Foundation.Services;

/// <summary>
/// Configuration options for the FTS URL service
/// </summary>
public class FtsUrlOptions
{
    /// <summary>
    /// The base URL of the FTS service
    /// </summary>
    public string? ServiceBaseUrl { get; set; }

    /// <summary>
    /// The session key used to store the FTS service origin
    /// </summary>
    public string SessionKey { get; set; } = "FtsServiceOrigin";
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
public class FtsUrlService : IFtsUrlService
{
    private readonly string _ftsService;
    private readonly ICookiePreferencesService? _cookiePreferencesService;

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
    {
        _cookiePreferencesService = cookiePreferencesService;

        var session = httpContextAccessor.HttpContext?.Session;
        var ftsService = session?.GetString(options.SessionKey)
                         ?? options.ServiceBaseUrl
                         ?? throw new InvalidOperationException("FTS service URL is not configured.");

        _ftsService = ftsService.TrimEnd('/');
    }

    /// <inheritdoc />
    public string BuildUrl(string endpoint, Guid? organisationId = null, string? redirectUrl = null)
    {
        var uriBuilder = new UriBuilder(_ftsService)
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