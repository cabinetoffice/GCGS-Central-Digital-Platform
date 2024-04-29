using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Registration;

public class OneLogin : PageModel
{
    [BindProperty]
    public string? UserId { get; set; }

    [BindProperty]
    public string? Email { get; set; }

    [BindProperty]
    public string? Phone { get; set; }

    public async Task<IActionResult> OnGet(string action)
    {
        return action switch
        {
            "sign-in" => SignIn(),
            "user-info" => await UserInfo(),
            "sign-out" => SignOut(),
            _ => RedirectToPage("/"),
        };
    }

    private IActionResult SignIn()
    {
        return Challenge(new AuthenticationProperties { RedirectUri = "/one-login/user-info" });
    }

    private async Task<IActionResult> UserInfo()
    {
        var userInfo = await HttpContext.AuthenticateAsync();
        if (!userInfo.Succeeded)
        {
            return SignIn();
        }

        UserId = userInfo.Principal.FindFirst(JwtClaimTypes.Subject)?.Value;
        Email = userInfo.Principal.FindFirst(JwtClaimTypes.Email)?.Value;
        Phone = userInfo.Principal.FindFirst(JwtClaimTypes.PhoneNumber)?.Value;

        return Page();
    }

    private IActionResult SignOut()
    {
        if (User?.Identity?.IsAuthenticated != true)
        {
            return Redirect("/");
        }

        return SignOut(new AuthenticationProperties { RedirectUri = "/" },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
    }
}