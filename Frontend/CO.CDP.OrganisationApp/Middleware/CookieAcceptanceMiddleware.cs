namespace CO.CDP.OrganisationApp.Middleware;

public class CookieAcceptanceMiddleware(ICookiePreferencesService cookiePreferencesService) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Query.TryGetValue(CookieSettings.FtsHandoverParameter, out var cookiesAcceptedValue))
        {
            string cookiesAccepted = cookiesAcceptedValue.ToString().ToLower();

            switch (cookiesAccepted)
            {
                case "true":
                    cookiePreferencesService.Accept();
                    break;
                case "false":
                    cookiePreferencesService.Reject();
                    break;
                case "unknown":
                    cookiePreferencesService.Reset();
                    break;
            }
        }

        await next(context);
    }
}
