using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages;

[AuthenticatedSessionNotRequired]
public class CookiesModel(
    IWebHostEnvironment env,
    ITempDataService tempDataService) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage="Please indicate whether you accept cookies that measure website use")]
    public CookieAcceptanceValues? CookieAcceptance { get; set; }

    [BindProperty]
    public string? ReturnUrl { get; set; }

    public void OnGet()
    {
        var cookieValue = Request.Cookies[CookieSettings.CookieName];

        if (cookieValue != null)
        {
            if (Enum.TryParse(cookieValue, out CookieAcceptanceValues result))
            {
                CookieAcceptance = result;
            }
        }
    }

    public IActionResult OnPost()
    {
        if(!ModelState.IsValid) {
            return Page();
        }

        Response.Cookies.Append(CookieSettings.CookieName, ((int)CookieAcceptance).ToString(), new CookieOptions{
            Expires = DateTimeOffset.UtcNow.AddDays(365),
            IsEssential = true,
            HttpOnly = false,
            Secure = !env.IsDevelopment()
        });

        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(QueryHelpers.AddQueryString(ReturnUrl, CookieSettings.CookiesAcceptedQueryString, "true"));
        }
        tempDataService.Put(FlashMessageTypes.Success, new FlashMessage(
            "You’ve set your cookie preferences."
        ));

        return RedirectToPage("/Cookies");
    }

    public bool RadioIsChecked(CookieAcceptanceValues value)
    {
        return Request.Cookies.ContainsKey(CookieSettings.CookieName) && Request.Cookies[CookieSettings.CookieName] == ((int)value).ToString();
    }
}

public enum CookieAcceptanceValues
{
    Accept=1,
    Reject=2
}

public static class CookieSettings
{
    public const string CookieAcceptanceFieldName = "CookieAcceptance";
    public const string CookieSettingsPageReturnUrlFieldName = "ReturnUrl";
    public const string CookiesAcceptedQueryString = "cookiesAccepted";

    // Cookie name in FTS is FT_COOKIES_PREFERENCES_SET
    // Cookie values have been configured to match, so if we're sharing a domain in production,
    // we could switch to using the same cookie name and remove the frontend page (But keep the post handler for setting it?)
    public const string CookieName = "SIRSI_COOKIES_PREFERENCES_SET";
}