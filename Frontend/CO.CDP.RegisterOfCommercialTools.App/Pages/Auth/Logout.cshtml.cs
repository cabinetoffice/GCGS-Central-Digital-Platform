using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.RegisterOfCommercialTools.App.Pages.Auth;

public class LogoutModel : PageModel
{
    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return SignOut(
                new AuthenticationProperties 
                { 
                    RedirectUri = "/" 
                },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme
            );
        }
        
        return Redirect("/");
    }
}