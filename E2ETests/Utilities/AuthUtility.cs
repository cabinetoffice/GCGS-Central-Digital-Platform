using Microsoft.Playwright;

namespace E2ETests.Utilities;

public static class AuthUtility
{
    // XPath: grab the token for the "Authority Access Token" row only
    private const string AccessTokenSelector = "div#aat"; // Selector for access token

    private static string Join(string baseUrl, string path)
        => $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";

    public static async Task<string> GetAccessToken(IPage page)
    {
        var baseUrl       = ConfigUtility.GetBaseUrl(); // <- from appsettings/env
        var diagnosticUrl = Join(baseUrl, "diagnostic");
        var dashboardUrl  = Join(baseUrl, "organisation-selection");

        await page.GotoAsync(diagnosticUrl);
        await page.WaitForSelectorAsync(AccessTokenSelector);

        var accessToken = await page.InnerTextAsync(AccessTokenSelector);
        Console.WriteLine($"✅ Extracted Access Token: {accessToken}");

        await page.GotoAsync(dashboardUrl);
        return accessToken;
    }
}