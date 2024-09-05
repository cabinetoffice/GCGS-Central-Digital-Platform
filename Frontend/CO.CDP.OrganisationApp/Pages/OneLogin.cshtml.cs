using CO.CDP.OrganisationApp.Models;
using CO.CDP.Person.WebApiClient;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[AuthorisedSessionNotRequired]
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

            var personInviteId = session.Get<Guid?>("PersonInviteId");

            if (personInviteId != null)
            {
                await ClaimPersonInvite(person.Id, personInviteId.Value);
            }

            session.Remove("PersonInviteId");

            return RedirectToPage("OrganisationSelection");
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            session.Remove("PersonInviteId");

            return RedirectToPage("PrivacyPolicy");
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

    private async Task ClaimPersonInvite(Guid personId, Guid personInviteId)
    {
        var command = new ClaimPersonInvite(personInviteId);
        await personClient.ClaimPersonInviteAsync(personId, command);
    }
}