using Microsoft.Playwright;

namespace E2ETests.Utilities;

public static class AuthUtility
{
    private const string DiagnosticUrl = "http://localhost:8090/diagnostic";
    private const string AccessTokenSelector = "div#aat"; // Selector for access token
    private const string DashboardUrl = "http://localhost:8090/organisation-selection";

    public static async Task<string> GetAccessToken(IPage page)
    {
        // Navigate to the diagnostic page
        await page.GotoAsync(DiagnosticUrl);
        await page.WaitForSelectorAsync(AccessTokenSelector);

        // Extract the access token
        var accessToken = await page.InnerTextAsync(AccessTokenSelector);
        Console.WriteLine($"✅ Extracted Access Token: {accessToken}");
        await page.GotoAsync(DashboardUrl);
        return accessToken;
    }
}
