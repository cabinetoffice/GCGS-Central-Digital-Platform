using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages;

[AuthenticatedSessionNotRequired]
public class ChangeLanguageModel(ITempDataService tempDataService) : PageModel
{
    public IActionResult OnGet(string culture, string? returnUrl)
    {
        if (!string.IsNullOrEmpty(culture))
        {
            Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    Path = "/",
                    Secure = Request.IsHttps,
                    HttpOnly = true
                });

            foreach (var key in tempDataService.Keys.ToList())
            {
                if (key.StartsWith($"Form_") && key.EndsWith("_Questions"))
                {
                    tempDataService.Remove(key);
                }
            }
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return Redirect("/");
    }
}
