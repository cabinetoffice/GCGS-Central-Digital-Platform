using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages.Users;

/// <summary>
/// Page object for the users list page in the UserManagement.App.
/// URL pattern: /organisation/{guid}
/// </summary>
public class UserManagementUsersListPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

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

    public async Task<string> GetCurrentUrl() => _page.Url;
}
