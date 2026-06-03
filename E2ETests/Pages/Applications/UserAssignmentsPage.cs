using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages.Applications;

/// <summary>
/// Page Object Model for /organisation/{id}/applications/{appId}/user-assignments
/// </summary>
public class UserAssignmentsPage(IPage page)
{
    private readonly string _baseUrl = ConfigUtility.GetBaseUrl();

    // Selectors
    private const string HeadingSelector         = "h1.govuk-heading-xl";
    private const string AssignUserButtonSelector = "a.govuk-button:has-text('Assign user')";
    private const string TableSelector            = "#assignments-table";
    private const string TableRowSelector         = "#assignments-table tbody tr";
    private const string SearchInputSelector      = "input[id='user-search']";
    private const string EditRolesLinkPattern     = "a.govuk-link:has-text('Edit roles')";
    private const string RevokeLinkPattern        = "a.govuk-link:has-text('Revoke')";
    private const string NoUsersMessageSelector   = "p.govuk-body";

    public async Task<string> GetHeading()
        => await page.InnerTextAsync(HeadingSelector);

    public async Task<bool> AssignUserButtonIsVisible()
        => await page.IsVisibleAsync(AssignUserButtonSelector);

    public async Task ClickAssignUser()
    {
        await page.ClickAsync(AssignUserButtonSelector);
        await page.WaitForURLAsync(url => url.Contains("/assign"));
    }

    public async Task<bool> TableIsVisible()
        => await page.IsVisibleAsync(TableSelector);

    public async Task<int> GetAssignmentCount()
    {
        var rows = page.Locator(TableRowSelector);
        return await rows.CountAsync();
    }

    public async Task<IReadOnlyList<string>> GetAssignedUserPrincipals()
    {
        var cells = page.Locator($"{TableRowSelector} td:first-child code");
        var count = await cells.CountAsync();
        var users = new List<string>();
        for (int i = 0; i < count; i++)
            users.Add(await cells.Nth(i).InnerTextAsync());
        return users;
    }

    public async Task SearchUsers(string query)
    {
        await page.FillAsync(SearchInputSelector, query);
    }

    public async Task<int> GetVisibleRowCount()
    {
        var rows = page.Locator($"{TableRowSelector}[style!='display: none;']");
        return await rows.CountAsync();
    }

    public async Task ClickEditRoles(int rowIndex = 0)
    {
        var links = page.Locator(EditRolesLinkPattern);
        await links.Nth(rowIndex).ClickAsync();
        await page.WaitForURLAsync(url => url.Contains("edit-roles"));
    }

    public async Task ClickRevoke(int rowIndex = 0)
    {
        var links = page.Locator(RevokeLinkPattern);
        await links.Nth(rowIndex).ClickAsync();
        await page.WaitForURLAsync(url => url.Contains("/revoke"));
    }

    public async Task AssertUserAssigned(string partialUrn)
    {
        var cells = page.Locator($"{TableRowSelector} td:first-child code");
        var count = await cells.CountAsync();
        for (int i = 0; i < count; i++)
        {
            var text = await cells.Nth(i).InnerTextAsync();
            if (text.Contains(partialUrn)) return;
        }
        throw new AssertionException($"User with URN containing '{partialUrn}' not found in assignments table");
    }

    public async Task AssertUserNotAssigned(string partialUrn)
    {
        if (!await TableIsVisible()) return; // No table = no users, assertion passes
        var cells = page.Locator($"{TableRowSelector} td:first-child code");
        var count = await cells.CountAsync();
        for (int i = 0; i < count; i++)
        {
            var text = await cells.Nth(i).InnerTextAsync();
            if (text.Contains(partialUrn))
                throw new AssertionException($"User with URN '{partialUrn}' is still in the table but should have been revoked");
        }
    }

    public async Task<string> GetPageUrl() => page.Url;
}
