using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages.Applications;

/// <summary>
/// Page Object Model for /organisation/{id}/applications/{appId} — Application detail with Roles/Permissions tabs.
/// </summary>
public class ApplicationDetailPage(IPage page)
{
    private readonly string _baseUrl = ConfigUtility.GetBaseUrl();

    // Selectors
    private const string HeadingSelector              = "h1.govuk-heading-xl";
    private const string RolesTabSelector             = "a.govuk-tabs__tab[href='#roles']";
    private const string PermissionsTabSelector       = "a.govuk-tabs__tab[href='#permissions']";
    private const string RolesTableSelector           = "#roles table.govuk-table";
    private const string PermissionsTableSelector     = "#permissions table.govuk-table";
    private const string ManageUsersButtonSelector    = "a.govuk-button:has-text('Manage user assignments')";
    private const string ClientIdSelector             = "code";
    private const string SummaryListSelector          = "dl.govuk-summary-list";

    public async Task<string> GetHeading()
        => await page.InnerTextAsync(HeadingSelector);

    public async Task<bool> RolesTabIsVisible()
        => await page.IsVisibleAsync(RolesTabSelector);

    public async Task<bool> PermissionsTabIsVisible()
        => await page.IsVisibleAsync(PermissionsTabSelector);

    public async Task ClickRolesTab()
    {
        await page.ClickAsync(RolesTabSelector);
        await page.WaitForSelectorAsync(RolesTableSelector);
    }

    public async Task ClickPermissionsTab()
    {
        await page.ClickAsync(PermissionsTabSelector);
        await page.WaitForSelectorAsync(PermissionsTableSelector);
    }

    public async Task<int> GetRoleCount()
    {
        var rows = page.Locator($"{RolesTableSelector} tbody tr");
        return await rows.CountAsync();
    }

    public async Task<IReadOnlyList<string>> GetRoleNames()
    {
        var cells = page.Locator($"{RolesTableSelector} tbody tr td:first-child strong");
        var count = await cells.CountAsync();
        var names = new List<string>();
        for (int i = 0; i < count; i++)
            names.Add(await cells.Nth(i).InnerTextAsync());
        return names;
    }

    public async Task<bool> ManageUsersButtonIsVisible()
        => await page.IsVisibleAsync(ManageUsersButtonSelector);

    public async Task ClickManageUserAssignments()
    {
        await page.ClickAsync(ManageUsersButtonSelector);
        await page.WaitForURLAsync(url => url.Contains("user-assignments"));
    }

    public async Task<string> GetClientId()
        => await page.InnerTextAsync(ClientIdSelector);

    public async Task<string> GetPageUrl() => page.Url;
}
