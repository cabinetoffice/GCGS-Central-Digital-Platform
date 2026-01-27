using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CO.CDP.Authentication.Services;

public class CookieEventsService(ILogoutManager logoutManager) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        if (context.Principal?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var urn = context.Principal.FindFirst("sub")?.Value;

        if (urn == null || !await logoutManager.HasLoggedOut(urn))
        {
            return;
        }

        await logoutManager.RemoveAsLoggedOut(urn);

        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync();
    }
}
