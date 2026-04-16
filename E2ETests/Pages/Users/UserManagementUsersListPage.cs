using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages.Users;

/// <summary>
/// Page object for the UserManagement.App organisation users list page
/// at /organisation/{guid} (served from a separate service when the UserManagement feature flag is on).
/// </summary>
public class UserManagementUsersListPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    private readonly string PageHeadingSelector = "h1.govuk-heading-l";

    public UserManagementUsersListPage(IPage page)
    {
        _page = page;
        _baseUrl = ConfigUtility.GetUserManagementAppBaseUrl();
    }

    public async Task NavigateTo(string organisationId)
    {
        var url = $"{_baseUrl}/organisation/{organisationId}";
        await _page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to UserManagement users list: {url}");
    }

    public async Task<string> GetPageHeading()
    {
        await _page.WaitForSelectorAsync(PageHeadingSelector);
        return await _page.InnerTextAsync(PageHeadingSelector);
    }

    public async Task<string> GetCurrentUrl() => _page.Url;
}
