using CO.CDP.OrganisationApp.Models;
using CO.CDP.Person.WebApiClient;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Registration;

public class OneLogin(
    IHttpContextAccessor httpContextAccessor,
    IPersonClient personClient,
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
        var userInfo = await httpContextAccessor.HttpContext!.AuthenticateAsync();
        if (!userInfo.Succeeded)
        {
            return SignIn();
        }

        var urn = userInfo.Principal.FindFirst(JwtClaimTypes.Subject)?.Value;
        var email = userInfo.Principal.FindFirst(JwtClaimTypes.Email)?.Value;
        var phone = userInfo.Principal.FindFirst(JwtClaimTypes.PhoneNumber)?.Value;

        if (urn == null)
        {
            return SignIn();
        }

        var rd = new RegistrationDetails
        {
            UserUrn = urn,
            Email = email,
            Phone = phone
        };

        Person.WebApiClient.Person? person;

        try
        {
            person = await personClient.LookupPersonAsync(urn);
            rd.PersonId = person.Id;
            rd.FirstName = person.FirstName;
            rd.LastName = person.LastName;

            session.Set(Session.RegistrationDetailsKey, rd);

            return RedirectToPage("Registration/OrganisationSelection");
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            session.Set(Session.RegistrationDetailsKey, rd);

            return RedirectToPage("PrivacyPolicy");
        }
    }

    private IActionResult SignOut()
    {
        if (httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return RedirectToPage("/");
        }

        return SignOut(new AuthenticationProperties { RedirectUri = "/" },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
    }
}
