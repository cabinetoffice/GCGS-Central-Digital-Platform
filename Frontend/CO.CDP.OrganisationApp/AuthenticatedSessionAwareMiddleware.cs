using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages;

namespace CO.CDP.OrganisationApp;

public class AuthenticatedSessionAwareMiddleware(RequestDelegate next, ISession session)
{
    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Path != "/health")
        {
            var endpoint = context.GetEndpoint();

            if (endpoint != null)
            {
                if (endpoint.Metadata.GetMetadata<AuthenticatedSessionNotRequiredAttribute>() is null)
                {
                    if (context.User.Identity?.IsAuthenticated == false)
                    {
                        context.Response.Redirect("/");
                        return;
                    }

                    var details = session.Get<UserDetails>(Session.UserDetailsKey);

                    if (details == null)
                    {
                        context.Response.Redirect("/");
                        return;
                    }
                }
            }
        }

        await next.Invoke(context);
    }
}