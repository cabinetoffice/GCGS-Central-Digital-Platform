using CO.CDP.OrganisationApp.Models;
using CO.CDP.Person.WebApiClient;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[AuthorisedSessionNotRequired]
public class OneLogin(
    IHttpContextAccessor httpContextAccessor,
    IPersonClient personClient,
    ISession session) : PageModel
{
    public async Task<IActionResult> OnGet(string action, string? redirectUri = null)
    {
        return action switch
        {
            "sign-in" => SignIn(redirectUri),
            "user-info" => await UserInfo(redirectUri),
            "sign-out" => SignOut(),
            _ => RedirectToPage("/"),
        };
    }

    private IActionResult SignIn(string? redirectUri = null)
    {
        var uri = "/one-login/user-info";
        if (Helper.ValidRelativeUri(redirectUri))
        {
            uri += $"?redirectUri={WebUtility.UrlEncode(redirectUri)}";
        }

        return Challenge(new AuthenticationProperties { RedirectUri = uri });
    }

    private async Task<IActionResult> UserInfo(string? redirectUri = null)
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

        var ud = new UserDetails { UserUrn = urn, Email = email, Phone = phone };
        session.Set(Session.UserDetailsKey, ud);

        Person.WebApiClient.Person? person;

        try
        {
            person = await personClient.LookupPersonAsync(urn);

            ud = session.Get<UserDetails>(Session.UserDetailsKey);
            if (ud != null)
            {
                ud.PersonId = person.Id;
                ud.FirstName = person.FirstName;
                ud.LastName = person.LastName;
                session.Set(Session.UserDetailsKey, ud);
            }

            if (Helper.ValidRelativeUri(redirectUri))
            {
                return Redirect(redirectUri!);
            }

            return RedirectToPage("OrganisationSelection");
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return RedirectToPage("PrivacyPolicy", new { RedirectUri = Helper.ValidRelativeUri(redirectUri) ? redirectUri : default });
        }
    }

    private IActionResult SignOut()
    {
        session.Clear();
        if (httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return RedirectToPage("/");
        }

        return SignOut(new AuthenticationProperties { RedirectUri = "/" },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
    }
}