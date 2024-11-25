using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CO.CDP.OrganisationApp.Authentication;

public class CookieEvents(
    ILogoutManager logoutManager,
    IAuthorityClient authorityClient) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        if (context.Principal?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var urn = context.Principal.FindFirst("sub")?.Value;

        if (urn == null || await logoutManager.HasLoggedOut(urn) == false)
        {
            return;
        }

        await logoutManager.RemoveAsLoggedOut(urn);
        await authorityClient.RevokeRefreshToken(urn);

        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync();
    }
}