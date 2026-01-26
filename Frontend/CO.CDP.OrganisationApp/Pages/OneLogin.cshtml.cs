using CO.CDP.Authentication.Services;
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
using Microsoft.FeatureManagement;
using System.Net;
using System.Web;
using CO.CDP.OrganisationApp.Authentication;

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
    ILogger<OneLoginModel> logger,
    IFeatureManager featureManager,
    IConfiguration config) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public required string PageAction { get; set; }

    public async Task<IActionResult> OnGetAsync(string? redirectUri = null, string? origin = null)
    {
        return PageAction.ToLower() switch
        {
            "sign-in" => await SignIn(redirectUri, origin),
            "user-info" => await UserInfo(redirectUri),
            "sign-out" => await SignOut(redirectUri),
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

    private async Task<ChallengeResult> SignIn(string? redirectUri = null, string? origin = null)
    {
        var uri = "/one-login/user-info";

        if (Helper.ValidRelativeUri(redirectUri))
        {
            var uriBuilder = new UriBuilder("https://example.com" + redirectUri);

            if (!string.IsNullOrWhiteSpace(origin) && await featureManager.IsEnabledAsync(FeatureFlags.AllowDynamicFtsOrigins))
            {
                var allowedOrigins = config["FtsServiceAllowedOrigins"] ?? "";
                if (allowedOrigins.Split(",", StringSplitOptions.RemoveEmptyEntries).Contains(origin))
                {
                    var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                    query["origin"] = origin;
                    uriBuilder.Query = query.ToString();
                }
            }

            uri += $"?redirectUri={WebUtility.UrlEncode(uriBuilder.Uri.PathAndQuery)}";
        }

        session.Remove(Session.UserAuthTokens);
        return Challenge(new AuthenticationProperties { RedirectUri = uri });
    }

    private async Task<IActionResult> UserInfo(string? redirectUri = null)
    {
        var userInfo = await httpContextAccessor.HttpContext!.AuthenticateAsync();
        if (!userInfo.Succeeded)
        {
            return await SignIn(redirectUri);
        }

        var urn = userInfo.Principal.FindFirst(JwtClaimTypes.Subject)?.Value;
        var email = userInfo.Principal.FindFirst(JwtClaimTypes.Email)?.Value;
        var phone = userInfo.Principal.FindFirst(JwtClaimTypes.PhoneNumber)?.Value;

        if (urn == null)
        {
            return await SignIn(redirectUri);
        }
        await logoutManager.RemoveAsLoggedOut(urn);
        SetFtsOrigin(redirectUri);

        var ud = new UserDetails { UserUrn = urn, Email = email, Phone = phone };
        session.Set(Session.UserDetailsKey, ud);

        Person.WebApiClient.Person? person;

        try
        {
            person = await LookupPerson(ud);

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

    private async Task<Person.WebApiClient.Person> LookupPerson(UserDetails ud)
    {
        try
        {
            // Look up by URN first
            return await personClient.LookupPersonAsync(urn: ud.UserUrn, email: null);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            logger.LogInformation("Person not found by URN {URN}, looking up by email.", ud.UserUrn);
        }

        try
        {
            // Look up by email second
            // If the user has deleted their one login account and recreated, they will come back to us with the same email but a different URN
            var person = await personClient.LookupPersonAsync(urn: null, email: ud.Email);

            await personClient.UpdatePersonAsync(person.Id, new UpdatedPerson(ud.UserUrn));

            return person;
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            logger.LogInformation("Person not found by email.");
            throw;
        }
    }

    private void SetFtsOrigin(string? redirectUri)
    {
        session.Remove(Session.FtsServiceOrigin);

        try
        {
            if (!string.IsNullOrWhiteSpace(redirectUri))
            {
                var uri = new Uri("https://example.com" + redirectUri);
                var origin = HttpUtility.ParseQueryString(uri.Query).Get("origin");
                if (!string.IsNullOrWhiteSpace(origin))
                {
                    session.Set(Session.FtsServiceOrigin, origin);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning("Invalid redirectUri, {Excption}", ex);
        }
    }

    private async Task<IActionResult> SignOut(string? redirectUriArgument)
    {
        var ud = session.Get<UserDetails>(Session.UserDetailsKey);
        if (!string.IsNullOrWhiteSpace(ud?.UserUrn))
            await authorityClient.RevokeRefreshToken(ud.UserUrn);

        session.Clear();

        var redirectUri = !string.IsNullOrEmpty(redirectUriArgument) && Helper.ValidRelativeUri(redirectUriArgument) ? redirectUriArgument : "/user/signedout";

        if (httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return Redirect(redirectUri);
        }

        return SignOut(new AuthenticationProperties { RedirectUri = redirectUri },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
    }
}