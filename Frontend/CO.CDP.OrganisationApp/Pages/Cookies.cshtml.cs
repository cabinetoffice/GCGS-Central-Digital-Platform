using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages;

[AuthenticatedSessionNotRequired]
public class CookiesModel(
    IFlashMessageService flashMessageService,
    ICookiePreferencesService cookiePreferencesService) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage="Choose whether you accept cookies that measure website use")]
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

        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(QueryHelpers.AddQueryString(ReturnUrl, CookieSettings.CookieBannerInteractionQueryString, "true"));
        }

        flashMessageService.SetFlashMessage(FlashMessageType.Success, "You’ve set your cookie preferences.");
        
        return RedirectToPage("/Cookies");
    }
}