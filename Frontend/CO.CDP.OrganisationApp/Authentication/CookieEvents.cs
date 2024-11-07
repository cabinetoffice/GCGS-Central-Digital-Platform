using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CO.CDP.OrganisationApp.Authentication;

public class CookieEvents(IOneLoginSessionManager oneLoginSessionManager) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        if (context.Principal?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var urn = context.Principal.FindFirst("sub")?.Value;

        if (urn == null || !oneLoginSessionManager.IsLoggedOut(urn))
        {
            return;
        }

        oneLoginSessionManager.RemoveUserLoggedOut(urn);

        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync();
    }
}