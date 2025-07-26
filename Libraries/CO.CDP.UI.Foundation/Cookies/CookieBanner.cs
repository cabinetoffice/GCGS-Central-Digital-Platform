using CO.CDP.UI.Foundation.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Http.Extensions;

namespace CO.CDP.UI.Foundation.Cookies;

/// <summary>
/// Component for rendering a GOV.UK compliant cookie banner
/// </summary>
public class CookieBanner
{
    private readonly string _serviceName;
    private readonly string _cookieInfoUrl;
    private readonly string _cookieInfoPath;
    private readonly ICookiePreferencesService _cookiePreferencesService;
    private readonly HttpContext _httpContext;
    private readonly CookieSettings _cookieSettings;

    /// <summary>
    /// Creates a new cookie banner component
    /// </summary>
    /// <param name="cookiePreferencesService">Service to manage cookie preferences</param>
    /// <param name="httpContextAccessor">HTTP context accessor for request information</param>
    /// <param name="cookieSettings">Cookie settings configuration</param>
    /// <param name="sirsiUrlService">The Sirsi URL service</param>
    /// <param name="serviceName">The name of the service to display in the banner</param>
    private CookieBanner(
        ICookiePreferencesService cookiePreferencesService,
        IHttpContextAccessor httpContextAccessor,
        CookieSettings cookieSettings,
        ISirsiUrlService sirsiUrlService,
        string serviceName = "Find a Tender")
    {
        _cookiePreferencesService = cookiePreferencesService;
        _httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HTTP context.");
        _cookieSettings = cookieSettings;
        _serviceName = serviceName;
        _cookieInfoUrl = sirsiUrlService.BuildUrl("/cookies");
        _cookieInfoPath = sirsiUrlService.GetPath("/cookies");
    }

    /// <summary>
    /// Renders the cookie banner as HTML
    /// </summary>
    /// <returns>HTML markup for the cookie banner</returns>
    private HtmlString Render()
    {
        if (ShouldShowInitialBanner())
        {
            return RenderInitialBanner(BuildCleanReturnUrl());
        }

        if (ShouldShowConfirmationBanner())
        {
            return RenderConfirmationBanner(BuildCleanQueryStringUrl());
        }

        return new HtmlString(string.Empty);
    }

    /// <summary>
    /// Determines if the initial cookie banner should be shown
    /// </summary>
    private bool ShouldShowInitialBanner()
    {
        return _cookiePreferencesService.IsUnknown() &&
               !_httpContext.Request.Query.ContainsKey(_cookieSettings.CookieBannerInteractionQueryString) &&
               !_httpContext.Request.Path.StartsWithSegments(_cookieInfoPath);
    }

    /// <summary>
    /// Determines if the confirmation banner should be shown
    /// </summary>
    private bool ShouldShowConfirmationBanner()
    {
        return !_cookiePreferencesService.IsUnknown() &&
               _httpContext.Request.Query.ContainsKey(_cookieSettings.CookieBannerInteractionQueryString);
    }

    /// <summary>
    /// Builds a clean return URL by removing FTS handover parameters
    /// </summary>
    private string BuildCleanReturnUrl()
    {
        // Get the current URL with FTS handover parameter removed for the return URL
        var queryDict = QueryHelpers.ParseQuery(_httpContext.Request.QueryString.Value);
        queryDict.Remove(_cookieSettings.FtsHandoverParameter);
        var queryBuilder = new QueryBuilder(queryDict);
        var updatedQueryString = queryBuilder.ToQueryString();
        return _httpContext.Request.PathBase + _httpContext.Request.Path + updatedQueryString;
    }

    /// <summary>
    /// Builds a clean URL by removing cookie banner interaction parameters
    /// </summary>
    private string BuildCleanQueryStringUrl()
    {
        var currentQueryString = _httpContext.Request.QueryString.ToString();
        var cleanQueryString = currentQueryString.Replace($"{_cookieSettings.CookieBannerInteractionQueryString}=true", "");
        cleanQueryString = cleanQueryString == "?" ? "" : cleanQueryString;
        return _httpContext.Request.PathBase + _httpContext.Request.Path + cleanQueryString;
    }

    /// <summary>
    /// Renders the initial cookie banner with options to accept or reject cookies
    /// </summary>
    /// <param name="returnUrl">The URL to return to after making a selection</param>
    private HtmlString RenderInitialBanner(string returnUrl)
    {
        var html = $@"
        <div class=""govuk-cookie-banner"" data-nosnippet role=""region"" aria-label=""Cookies on {_serviceName}"">
            <div class=""govuk-cookie-banner__message govuk-width-container"">
                <div class=""govuk-grid-row"">
                    <div class=""govuk-grid-column-two-thirds"">
                        <h2 class=""govuk-cookie-banner__heading govuk-heading-m"">
                            Cookies on {_serviceName}
                        </h2>
                        <div class=""govuk-cookie-banner__content"">
                            <p class=""govuk-body"">We use some essential cookies to make this service work.</p>
                            <p class=""govuk-body"">We'd also like to use analytics cookies so we can understand how you use the service and make improvements.</p>
                        </div>
                    </div>
                </div>

                <form action=""{_cookieInfoUrl}"" method=""post"">
                    <input type=""hidden"" name=""{_cookieSettings.CookieSettingsPageReturnUrlFieldName}"" value=""{returnUrl}"" />

                    <div class=""govuk-button-group"">
                        <button type=""submit"" class=""govuk-button"" name=""{_cookieSettings.CookieAcceptanceFieldName}"" value=""{CookieAcceptanceValues.Accept}"">Accept analytics cookies</button>
                        <button type=""submit"" class=""govuk-button"" name=""{_cookieSettings.CookieAcceptanceFieldName}"" value=""{CookieAcceptanceValues.Reject}"">Reject analytics cookies</button>
                        <a class=""govuk-link"" href=""{_cookieInfoUrl}"">View cookies</a>
                    </div>
                </form>
            </div>
        </div>";

        return new HtmlString(html);
    }

    /// <summary>
    /// Renders the confirmation banner after the user has made a cookie preference choice
    /// </summary>
    /// <param name="returnUrl">The URL to return to after dismissing the banner</param>
    private HtmlString RenderConfirmationBanner(string returnUrl)
    {
        var acceptedRejectedStatement = _cookiePreferencesService.IsAccepted()
            ? "You've accepted analytics cookies."
            : "You've rejected analytics cookies.";

        var html = $@"
        <div class=""govuk-cookie-banner"" data-nosnippet role=""region"" aria-label=""Cookies on {_serviceName}"">
            <div class=""govuk-cookie-banner__message govuk-width-container"">
                <div class=""govuk-grid-row"">
                    <div class=""govuk-grid-column-two-thirds"">
                        <div class=""govuk-cookie-banner__content"">
                            <p class=""govuk-body"">
                                {acceptedRejectedStatement} You can <a class=""govuk-link"" href=""{_cookieInfoUrl}"">change your cookie settings</a> at any time.
                            </p>
                        </div>
                    </div>
                </div>
                <div class=""govuk-button-group"">
                    <a class=""govuk-button"" role=""button"" data-module=""govuk-button"" href=""{returnUrl}"">Hide cookie message</a>
                </div>
            </div>
        </div>";

        return new HtmlString(html);
    }

    /// <summary>
    /// Creates and renders a cookie banner
    /// </summary>
    /// <param name="cookiePreferencesService">Service to manage cookie preferences</param>
    /// <param name="httpContextAccessor">HTTP context accessor for request information</param>
    /// <param name="cookieSettings">Cookie settings configuration</param>
    /// <param name="sirsiUrlService">The Sirsi URL service</param>
    /// <param name="serviceName">The name of the service to display in the banner</param>
    /// <returns>HTML markup for the cookie banner</returns>
    public static HtmlString RenderCookieBanner(
        ICookiePreferencesService cookiePreferencesService,
        IHttpContextAccessor httpContextAccessor,
        CookieSettings cookieSettings,
        ISirsiUrlService sirsiUrlService,
        string serviceName = "Find a Tender")
        {
        var banner = new CookieBanner(
            cookiePreferencesService,
            httpContextAccessor,
            cookieSettings,
            sirsiUrlService,
            serviceName);
        return banner.Render();
    }
}