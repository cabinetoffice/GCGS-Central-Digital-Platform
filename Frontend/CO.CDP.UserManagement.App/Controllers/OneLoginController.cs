using CO.CDP.Authentication.Services;
using CO.CDP.UserManagement.App.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.UserManagement.App.Controllers;

[ApiController]
public class OneLoginController(
    IOneLoginAuthority oneLoginAuthority,
    ILogoutManager logoutManager,
    ILogger<OneLoginController> logger) : ControllerBase
{
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [HttpPost("/signout-oidc")]
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
