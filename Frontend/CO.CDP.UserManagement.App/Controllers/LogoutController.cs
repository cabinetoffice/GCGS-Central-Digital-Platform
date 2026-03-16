using CO.CDP.UI.Foundation.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.UserManagement.App.Controllers;

[AllowAnonymous]
public class LogoutController(ISirsiUrlService sirsiUrlService) : Controller
{
    [HttpGet("/logout")]
    [HttpGet("/Auth/Logout")]
    public IActionResult Index()
    {
        var sirsiHomePage = sirsiUrlService.BuildUrl("/");

        if (User.Identity?.IsAuthenticated == true)
        {
            return SignOut(
                new AuthenticationProperties
                {
                    RedirectUri = sirsiHomePage
                },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme
            );
        }

        return Redirect(sirsiHomePage);
    }
}
