using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
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
            "sign-in-callback" => await SignIn(),
            "sign-out" => SignOut(),
            _ => RedirectToPage("/"),
        };
    }

    private async Task<IActionResult> SignIn()
    {
        UserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Email = HttpContext.User.FindFirst("email")?.Value;
        Phone = HttpContext.User.FindFirst("phone_number")?.Value;

        return Page();
    }

    private IActionResult SignOut()
    {
        return SignOut(new AuthenticationProperties { RedirectUri = "/" },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
    }
}