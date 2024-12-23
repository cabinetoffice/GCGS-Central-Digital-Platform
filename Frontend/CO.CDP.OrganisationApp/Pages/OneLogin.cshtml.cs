using CO.CDP.OrganisationApp.Authentication;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using CO.CDP.Person.WebApiClient;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace CO.CDP.OrganisationApp.Pages;

[AuthenticatedSessionNotRequired]
[IgnoreAntiforgeryToken]
public class OneLoginModel(
    IHttpContextAccessor httpContextAccessor,
    IPersonClient personClient,
    ISession session,
    ILogoutManager logoutManager,
    IOneLoginAuthority oneLoginAuthority,
    IAuthorityClient authorityClient,
    ILogger<OneLoginModel> logger) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public required string PageAction { get; set; }

    public async Task<IActionResult> OnGetAsync(string? redirectUri = null)
    {
        return PageAction.ToLower() switch
        {
            "sign-in" => SignIn(redirectUri),
            "user-info" => await UserInfo(redirectUri),
            "sign-out" => await SignOut(),
            _ => Redirect("/"),
        };
    }

    public async Task<IActionResult> OnPostAsync(string logout_token)
    {
        if (!PageAction.Equals("back-channel-sign-out", StringComparison.CurrentCultureIgnoreCase))
        {
            logger.LogInformation("One Login page post endpoint returns BadRequest response. {PageAction}", PageAction);
            return BadRequest("Invalid page request");
        }

        if (string.IsNullOrWhiteSpace(logout_token))
        {
            logger.LogInformation("One Login page post endpoint returns BadRequest response. {Token}", logout_token);
            return BadRequest("Missing token");
        }

        var urn = await oneLoginAuthority.ValidateLogoutToken(logout_token);

        if (string.IsNullOrWhiteSpace(urn))
        {
            logger.LogInformation("One Login page post endpoint returns BadRequest response, because unable to validate logout token. {Token}", logout_token);
            return BadRequest("Invalid token");
        }

        await logoutManager.MarkAsLoggedOut(urn, logout_token);

        logger.LogInformation("One Login page post endpoint process request successfully and added user {URN} to the signed-out sessions list. {Token}",
            urn, logout_token);

        return Page();
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
        await logoutManager.RemoveAsLoggedOut(urn);

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

            if (person.Scopes.Contains(PersonScopes.SupportAdmin))
            {
                return RedirectToPage("Support/Organisations", new Dictionary<string, string> { { "type", "buyer" } });
            }

            return RedirectToPage("Organisation/OrganisationSelection");
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return RedirectToPage("PrivacyPolicy", new { RedirectUri = Helper.ValidRelativeUri(redirectUri) ? redirectUri : default });
        }
    }

    private async Task<IActionResult> SignOut()
    {
        var ud = session.Get<UserDetails>(Session.UserDetailsKey);
        if (!string.IsNullOrWhiteSpace(ud?.UserUrn))
            await authorityClient.RevokeRefreshToken(ud.UserUrn);

        session.Clear();

        if (httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return Redirect("/");
        }

        return SignOut(new AuthenticationProperties { RedirectUri = "/" },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
    }
}