using CO.CDP.UI.Foundation.Cookies;
using Microsoft.AspNetCore.Http;

namespace CO.CDP.UI.Foundation.Middleware;

public class CookieAcceptanceMiddleware(ICookiePreferencesService cookiePreferencesService) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var cookieSettings = new CookieSettings();
        if (context.Request.Query.TryGetValue(cookieSettings.FtsHandoverParameter, out var cookiesAcceptedValue))
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
