using CO.CDP.Functional;
using CO.CDP.UI.Foundation.Cookies;
using Microsoft.AspNetCore.Http;

namespace CO.CDP.UI.Foundation.Middleware;

public class CookieAcceptanceMiddleware(ICookiePreferencesService cookiePreferencesService) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var cookieSettings = new CookieSettings();
        if (context.Request.Query.TryGetValue(cookieSettings.CookiesAcceptedHandoverParameter, out var cookiesAcceptedValue))
        {
            ParseCookieValue(cookiesAcceptedValue.ToString())
                .Match(
                    some: ApplyCookiePreference,
                    none: () => {});
        }

        await next(context);
    }

    private static Option<string> ParseCookieValue(string value)
    {
        var lowerValue = value.ToLower();
        return lowerValue is "true" or "false" or "unknown"
            ? Option<string>.Some(lowerValue)
            : Option<string>.None;
    }

    private void ApplyCookiePreference(string value)
    {
        switch (value)
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
}
