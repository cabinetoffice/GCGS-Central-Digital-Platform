using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CO.CDP.OrganisationApp.Authentication;

public class CookieEvents(
    IOneLoginSessionManager oneLoginSessionManager,
    IAuthorityClient authorityClient) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        if (context.Principal?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var urn = context.Principal.FindFirst("sub")?.Value;

        if (urn == null || await oneLoginSessionManager.HasSignedOut(urn) == false)
        {
            return;
        }

        await oneLoginSessionManager.RemoveFromSignedOutSessionsList(urn);
        await authorityClient.RevokeRefreshToken(urn);

        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync();
    }
}