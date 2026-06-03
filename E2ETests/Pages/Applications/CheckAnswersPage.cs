using Microsoft.Playwright;

namespace E2ETests.Pages.Applications;

/// <summary>
/// Page Object Model for /organisation/{id}/applications/{appId}/user-assignments/assign/check-answers
/// </summary>
public class CheckAnswersPage(IPage page)
{
    private const string HeadingSelector        = "h1.govuk-heading-l";
    private const string SummaryListSelector    = "dl.govuk-summary-list";
    private const string ConfirmButtonSelector  = "button:has-text('Confirm and save')";
    private const string RoleTagSelector        = "dd .govuk-tag--green";
    private const string ChangeUserLinkSelector = "a.govuk-link:has-text('Change')";

    public async Task<string> GetHeading()
        => await page.InnerTextAsync(HeadingSelector);

    public async Task<bool> SummaryIsVisible()
        => await page.IsVisibleAsync(SummaryListSelector);

    public async Task<string> GetApplicationValue()
    {
        var rows = page.Locator("dl.govuk-summary-list .govuk-summary-list__row");
        var count = await rows.CountAsync();
        for (int i = 0; i < count; i++)
        {
            var key = await rows.Nth(i).Locator(".govuk-summary-list__key").InnerTextAsync();
            if (key.Trim() == "Application")
                return await rows.Nth(i).Locator(".govuk-summary-list__value").InnerTextAsync();
        }
        return string.Empty;
    }

    public async Task<IReadOnlyList<string>> GetSelectedRoleNames()
    {
        var tags = page.Locator(RoleTagSelector);
        var count = await tags.CountAsync();
        var roles = new List<string>();
        for (int i = 0; i < count; i++)
            roles.Add(await tags.Nth(i).InnerTextAsync());
        return roles;
    }

    public async Task ClickConfirm()
    {
        await page.ClickAsync(ConfirmButtonSelector);
        await page.WaitForURLAsync(url => url.Contains("user-assignments") && !url.Contains("check-answers"));
    }

    public async Task<string> GetPageUrl() => page.Url;
}
