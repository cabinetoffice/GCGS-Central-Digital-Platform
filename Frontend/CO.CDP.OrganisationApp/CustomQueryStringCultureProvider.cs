using Microsoft.AspNetCore.Localization;

namespace CO.CDP.OrganisationApp;
public class CustomQueryStringCultureProvider : IRequestCultureProvider
{
    public Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var queryValue = httpContext.Request.Query["language"].ToString();
        if (!string.IsNullOrEmpty(queryValue))
        {
            var culture = GetCultureFromQuery(queryValue);
            if (culture != null)
            {
                httpContext.Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        IsEssential = true,
                        Path = "/",
                        Secure = httpContext.Request.IsHttps,
                        HttpOnly = true
                    }
                );

                return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture));
            }
        }

        return Task.FromResult<ProviderCultureResult?>(null);
    }

    private string GetCultureFromQuery(string queryValue)
    {
        if (queryValue.StartsWith("cy", StringComparison.OrdinalIgnoreCase))
            return "cy";

        return "en-GB";
    }
}
