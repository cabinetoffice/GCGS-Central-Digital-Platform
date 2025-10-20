using CO.CDP.Localization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace CO.CDP.UI.Foundation.Cookies;

/// <summary>
/// Immutable view model for the cookie banner partial view
/// Encapsulates all logic for determining banner visibility and generating URLs
/// </summary>
public record CookieBannerViewModel
{
    private readonly ICookiePreferencesService _cookiePreferencesService;
    private readonly CookieSettings _cookieSettings;
    private readonly IUrlHelper _urlHelper;
    private readonly string _path;
    private readonly string _pathBase;
    private readonly string _queryString;

    /// <summary>
    /// Initialises a new instance of the CookieBannerViewModel
    /// </summary>
    /// <param name="cookiePreferencesService">Service for managing cookie preferences</param>
    /// <param name="cookieSettings">Cookie configuration settings</param>
    /// <param name="urlHelper">URL helper for validating local URLs</param>
    /// <param name="path">Current request path</param>
    /// <param name="pathBase">Current request path base</param>
    /// <param name="queryString">Current request query string</param>
    public CookieBannerViewModel(
        ICookiePreferencesService cookiePreferencesService,
        CookieSettings cookieSettings,
        IUrlHelper urlHelper,
        string path,
        string pathBase,
        string? queryString)
    {
        _cookiePreferencesService = cookiePreferencesService;
        _cookieSettings = cookieSettings;
        _urlHelper = urlHelper;
        _path = EnsureLocalUrl(path, allowEmpty: false);
        _pathBase = EnsureLocalUrl(pathBase, allowEmpty: true);
        _queryString = queryString ?? string.Empty;
    }

    /// <summary>
    /// Determines whether to show the initial cookie consent banner
    /// </summary>
    /// <param name="hasInteractionQuery">Whether the interaction query parameter is present</param>
    /// <returns>True if the consent banner should be shown</returns>
    public bool ShouldShowConsentBanner(bool hasInteractionQuery) =>
        _cookiePreferencesService.IsUnknown()
        && !hasInteractionQuery
        && !IsOnCookiesPage();

    /// <summary>
    /// Determines whether to show the confirmation banner after user interaction
    /// </summary>
    /// <param name="hasInteractionQuery">Whether the interaction query parameter is present</param>
    /// <returns>True if the confirmation banner should be shown</returns>
    public bool ShouldShowConfirmationBanner(bool hasInteractionQuery) =>
        !_cookiePreferencesService.IsUnknown() && hasInteractionQuery;

    /// <summary>
    /// Gets the current URL with cookies accepted handover parameter removed
    /// Used as the return URL after cookie preferences are saved
    /// </summary>
    /// <returns>The current URL without handover parameter</returns>
    public string GetReturnUrl() =>
        BuildUrl(RemoveQueryParameter(_queryString, _cookieSettings.CookiesAcceptedHandoverParameter));

    /// <summary>
    /// Gets the URL for hiding the confirmation banner
    /// Removes the cookie banner interaction query parameter
    /// </summary>
    /// <returns>The current URL without the interaction parameter</returns>
    public string GetHideUrl() =>
        BuildUrl(RemoveQueryParameter(_queryString, _cookieSettings.CookieBannerInteractionQueryString));

    /// <summary>
    /// Gets the confirmation message text based on user's cookie preference
    /// </summary>
    /// <returns>The appropriate confirmation message</returns>
    public string GetConfirmationMessage() =>
        _cookiePreferencesService.IsAccepted()
            ? StaticTextResource.CookieBanner_AcceptedStatement
            : StaticTextResource.CookieBanner_RejectedStatement;

    /// <summary>
    /// Gets the field name for cookie acceptance input
    /// </summary>
    public string CookieAcceptanceFieldName => _cookieSettings.CookieAcceptanceFieldName;

    /// <summary>
    /// Gets the field name for return URL input
    /// </summary>
    public string ReturnUrlFieldName => _cookieSettings.CookieSettingsPageReturnUrlFieldName;

    private bool IsOnCookiesPage() =>
        _path.StartsWith("/cookies", StringComparison.OrdinalIgnoreCase);

    private string BuildUrl(string queryString) =>
        _pathBase + _path + queryString;

    private static string RemoveQueryParameter(string queryString, string parameterName)
    {
        var queryDict = QueryHelpers.ParseQuery(queryString);
        queryDict.Remove(parameterName);
        var queryBuilder = new QueryBuilder(queryDict);
        return queryBuilder.ToQueryString().ToString();
    }

    /// <summary>
    /// Ensures the URL is local using ASP.NET Core's validation.
    /// Returns the URL if valid, otherwise returns safe default.
    /// For paths: null/empty/invalid returns "/"
    /// For pathBase: null/invalid returns "", empty string is preserved
    /// </summary>
    /// <param name="url">URL to validate</param>
    /// <param name="allowEmpty">Whether empty string is valid (true for pathBase, false for path)</param>
    /// <returns>Validated local URL or safe default</returns>
    private string EnsureLocalUrl(string url, bool allowEmpty)
    {
        if (string.IsNullOrEmpty(url))
            return allowEmpty ? url : "/";

        return _urlHelper.IsLocalUrl(url) ? url : (allowEmpty ? string.Empty : "/");
    }
}
