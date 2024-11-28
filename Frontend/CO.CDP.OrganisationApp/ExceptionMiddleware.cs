namespace CO.CDP.OrganisationApp;

public class ExceptionMiddleware(
    RequestDelegate next,
    ILogger<ExceptionMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next.Invoke(context);

            // https://noticingsystem.atlassian.net/browse/DP-950
            // Remove below temp logging code of correlation cookie and state
            // once we identify the cause of login error in private-beta 
            if (context.Request.Path.StartsWithSegments("/one-login/sign-in"))
            {
                var cookies = context.Response.Headers.SetCookie;
                var location = context.Response.Headers.Location.ToString();
                string? state = null;
                string? correlationCookie = null;

                if (cookies != Microsoft.Extensions.Primitives.StringValues.Empty)
                {
                    correlationCookie = cookies.ToArray().FirstOrDefault(c => c!.StartsWith(".AspNetCore.Correlation."));
                }

                if (!string.IsNullOrWhiteSpace(location) && Uri.TryCreate(location, UriKind.Absolute, out var uri))
                {
                    state = System.Web.HttpUtility.ParseQueryString(uri.Query)["state"];
                }

                if (state != null && correlationCookie != null)
                {
                    logger.LogInformation("Correlation Cookie:{Cookie} set for state:{State}", correlationCookie, state);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            context.Response.Redirect("/error");
        }
    }
}