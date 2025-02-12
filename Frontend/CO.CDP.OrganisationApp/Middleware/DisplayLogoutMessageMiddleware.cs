using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace CO.CDP.OrganisationApp.Middleware;

public class DisplayLogoutMessageMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Cookies.ContainsKey(".AspNetCore.Cookies"))
        {
            var authResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (authResult == null
                || authResult.Failure?.Message == "Ticket expired"
                || authResult.Ticket?.Properties.ExpiresUtc < DateTimeOffset.UtcNow)
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                context.Response.Redirect("/logged-out-page");
                return;
            }
        }

        await next(context);
    }
}