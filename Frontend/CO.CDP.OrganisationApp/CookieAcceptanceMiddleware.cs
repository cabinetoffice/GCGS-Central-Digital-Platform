namespace CO.CDP.OrganisationApp;

public class CookieAcceptanceMiddleware(ICookiePreferencesService cookiePreferencesService) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Query.TryGetValue(CookieSettings.FtsHandoverParameter, out var cookiesAcceptedValue))
        {
            string cookiesAccepted = cookiesAcceptedValue.ToString().ToLower();

            if (cookiesAccepted == "true")
            {
                cookiePreferencesService.Accept();
            }
            else if (cookiesAccepted == "false")
            {
                cookiePreferencesService.Reject();
            }
            else if (cookiesAccepted == "unknown")
            {
                cookiePreferencesService.Reset();
            }
        }

        await next(context);
    }
}
