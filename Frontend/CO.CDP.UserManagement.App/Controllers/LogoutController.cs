using CO.CDP.Authentication.Services;
using CO.CDP.UserManagement.App.Authentication;
using CO.CDP.UI.Foundation.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.UserManagement.App.Controllers;

[AllowAnonymous]
public class LogoutController(
    ISirsiUrlService sirsiUrlService,
    IOneLoginAuthority oneLoginAuthority,
    ILogoutManager logoutManager,
    ILogger<LogoutController> logger) : Controller
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

    [IgnoreAntiforgeryToken]
    [HttpPost("/one-login/back-channel-sign-out")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> BackChannelSignOut([FromForm(Name = "logout_token")] string? logoutToken)
    {
        if (string.IsNullOrWhiteSpace(logoutToken))
        {
            logger.LogInformation("Back-channel sign-out called without logout token.");
            return BadRequest("Missing token");
        }

        var urn = await oneLoginAuthority.ValidateLogoutToken(logoutToken);
        if (string.IsNullOrWhiteSpace(urn))
        {
            logger.LogInformation("Back-channel sign-out called with invalid logout token.");
            return BadRequest("Invalid token");
        }

        await logoutManager.MarkAsLoggedOut(urn, logoutToken);
        logger.LogInformation("Back-channel sign-out processed for user {URN}.", urn);

        return Ok();
    }
}
