namespace CO.CDP.UI.Foundation.Cookies;

/// <summary>
/// Configuration settings for cookies across the application
/// </summary>
public class CookieSettings
{
    /// <summary>
    /// The name of the field used for cookie acceptance
    /// </summary>
    public string CookieAcceptanceFieldName { get; set; } = "CookieAcceptance";

    /// <summary>
    /// The name of the field used for the return URL when setting cookie preferences
    /// </summary>
    public string CookieSettingsPageReturnUrlFieldName { get; set; } = "ReturnUrl";

    /// <summary>
    /// The query string parameter for cookie banner interaction
    /// </summary>
    public string CookieBannerInteractionQueryString { get; set; } = "cookieBannerInteraction";

    /// <summary>
    /// The name of the cookie that stores preferences
    /// </summary>
    public string CookieName { get; set; } = "SIRSI_COOKIES_PREFERENCES_SET";

    /// <summary>
    /// The parameter name used when handing over to external services
    /// </summary>
    public string CookiesAcceptedHandoverParameter { get; set; } = "cookies_accepted";

    /// <summary>
    /// The number of days until the cookie expires
    /// </summary>
    public int ExpiryDays { get; set; } = 365;
}