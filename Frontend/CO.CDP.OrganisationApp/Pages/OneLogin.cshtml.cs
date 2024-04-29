using CO.CDP.OrganisationApp.Models;
using CO.CDP.Tenant.WebApiClient;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Registration;

public class OneLogin(
    ITenantClient tenantClient,
    ISession session) : PageModel
{
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

        var userId = userInfo.Principal.FindFirst(JwtClaimTypes.Subject)?.Value;
        var email = userInfo.Principal.FindFirst(JwtClaimTypes.Email)?.Value;
        var phone = userInfo.Principal.FindFirst(JwtClaimTypes.PhoneNumber)?.Value;

        Tenant.WebApiClient.Tenant? tenant;

        try
        {
            tenant = await tenantClient.LookupTenantAsync(userId);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            tenant = await tenantClient.CreateTenantAsync(
                   new NewTenant(
                       new TenantContactInfo(email, phone),
                       userId));
        }

        session.Set(Session.RegistrationDetailsKey,
        new RegistrationDetails
        {
            TenantId = tenant.Id,
            Email = tenant.ContactInfo.Email
        });

        return RedirectToPage("Registration/YourDetails");
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