using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace CO.CDP.OrganisationApp.Middleware;

public class DisplayLogoutMessageMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // If the request is not for the sign-in page, and the user is authenticated, and the cookie is expired,
        // then sign out the user and redirect to the logged-out page.
        if (context.Request.Path.StartsWithSegments("/signin-oidc", StringComparison.OrdinalIgnoreCase) == false
            && context.Request.Path.StartsWithSegments("/one-login", StringComparison.OrdinalIgnoreCase) == false
            && context.Request.Cookies.ContainsKey(".AspNetCore.Cookies"))
        {
            var authResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (authResult == null
                || authResult.Failure?.Message == "Ticket expired"
                || authResult.Ticket?.Properties.ExpiresUtc < DateTimeOffset.UtcNow)
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                context.Response.Redirect("/logged-out");
                return;
            }
        }

        await next(context);
    }
}