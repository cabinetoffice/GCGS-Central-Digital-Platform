using CO.CDP.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using CO.CDP.UI.Foundation.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.UI.Foundation.Pages;

[AllowAnonymous]
public class CookiesModel(
    ICookiePreferencesService cookiePreferencesService) : PageModel
{
    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Cookies_ChooseCookiePreferences), ErrorMessageResourceType = typeof(StaticTextResource))]
    public CookieAcceptanceValues? CookieAcceptance { get; set; }

    public void OnGet()
    {
        var cookieSettings = new CookieSettings();
        var cookieValue = Request.Cookies[cookieSettings.CookieName];

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
        if(!ModelState.IsValid || CookieAcceptance == null) {
            return Page();
        }

        switch(CookieAcceptance)
        {
            case CookieAcceptanceValues.Accept:
                cookiePreferencesService.Accept();
                break;

            case CookieAcceptanceValues.Reject:
                cookiePreferencesService.Reject();
                break;
        }

        return Redirect("/");
    }
}
