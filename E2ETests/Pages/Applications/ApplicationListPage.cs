using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages.Applications;

/// <summary>
/// Page Object Model for /organisation/{id}/applications — the Application Registry list page.
/// Accessible to Admin users of Buyer organisations only.
/// </summary>
public class ApplicationListPage(IPage page)
{
    private readonly string _baseUrl = ConfigUtility.GetBaseUrl();

    // Selectors
    private const string HeadingSelector         = "h1.govuk-heading-xl";
    private const string TableSelector            = "table.govuk-table";
    private const string AppRowSelector           = "table.govuk-table tbody tr";
    private const string AppLinkSelector          = "table.govuk-table tbody tr td a.govuk-link";
    private const string NoAppsMessageSelector    = "p.govuk-body";

    public async Task NavigateTo(string organisationId)
    {
        var url = $"{_baseUrl}/organisation/{organisationId}/applications";
        await page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to Application List: {url}");
        await page.WaitForSelectorAsync(HeadingSelector);
    }

    public async Task<string> GetHeading()
        => await page.InnerTextAsync(HeadingSelector);

    public async Task<bool> TableIsVisible()
        => await page.IsVisibleAsync(TableSelector);

    public async Task<int> GetApplicationCount()
    {
        var rows = page.Locator(AppRowSelector);
        return await rows.CountAsync();
    }

    public async Task<IReadOnlyList<string>> GetApplicationNames()
    {
        var links = page.Locator(AppLinkSelector);
        var count = await links.CountAsync();
        var names = new List<string>();
        for (int i = 0; i < count; i++)
            names.Add(await links.Nth(i).InnerTextAsync());
        return names;
    }

    public async Task ClickApplication(string appName)
    {
        await page.ClickAsync($"a.govuk-link:has-text(\"{appName}\")");
        await page.WaitForURLAsync(url => url.Contains("/applications/"));
    }

    public async Task<string> GetPageUrl() => page.Url;
}
