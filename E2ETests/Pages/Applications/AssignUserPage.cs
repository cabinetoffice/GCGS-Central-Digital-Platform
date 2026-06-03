using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages.Applications;

/// <summary>
/// Page Object Model for:
///   /organisation/{id}/applications/{appId}/user-assignments/assign  (new assignment)
///   /organisation/{id}/applications/{appId}/user-assignments/{userId}/edit-roles  (edit)
/// </summary>
public class AssignUserPage(IPage page)
{
    // Selectors
    private const string HeadingSelector          = "h1.govuk-heading-xl";
    private const string UserSelectSelector        = "select#SelectedUserPrincipalId";
    private const string UserHiddenInputSelector   = "input[name='SelectedUserPrincipalId']";
    private const string RoleCheckboxPattern       = "input[name='SelectedRoleIds']";
    private const string RoleLabelPattern          = "label.govuk-checkboxes__label";
    private const string ContinueButtonSelector    = "button:has-text('Continue')";
    private const string CancelLinkSelector        = "a.govuk-link:has-text('Cancel')";
    private const string ErrorSummarySelector      = ".govuk-error-summary";

    public async Task<string> GetHeading()
        => await page.InnerTextAsync(HeadingSelector);

    public async Task<bool> IsEditMode()
    {
        // In edit mode, user is displayed as read-only (no select, just code element)
        return !await page.IsVisibleAsync(UserSelectSelector);
    }

    public async Task SelectUser(string userPrincipalId)
    {
        await page.SelectOptionAsync(UserSelectSelector, new SelectOptionValue { Value = userPrincipalId });
    }

    public async Task<IReadOnlyList<string>> GetAvailableUserOptions()
    {
        var options = page.Locator($"{UserSelectSelector} option");
        var count = await options.CountAsync();
        var values = new List<string>();
        for (int i = 1; i < count; i++) // skip first "Choose a member" option
            values.Add(await options.Nth(i).GetAttributeAsync("value") ?? "");
        return values;
    }

    public async Task CheckRole(string roleName)
    {
        // Find checkbox by its associated label text
        var label = page.Locator($"label.govuk-checkboxes__label:has-text(\"{roleName}\")");
        var forAttr = await label.GetAttributeAsync("for");
        if (forAttr != null)
            await page.CheckAsync($"#{forAttr}");
    }

    public async Task UncheckRole(string roleName)
    {
        var label = page.Locator($"label.govuk-checkboxes__label:has-text(\"{roleName}\")");
        var forAttr = await label.GetAttributeAsync("for");
        if (forAttr != null)
            await page.UncheckAsync($"#{forAttr}");
    }

    public async Task<bool> IsRoleChecked(string roleName)
    {
        var label = page.Locator($"label.govuk-checkboxes__label:has-text(\"{roleName}\")");
        var forAttr = await label.GetAttributeAsync("for");
        return forAttr != null && await page.IsCheckedAsync($"#{forAttr}");
    }

    public async Task<IReadOnlyList<string>> GetAvailableRoleNames()
    {
        var labels = page.Locator($"{RoleLabelPattern} strong");
        var count  = await labels.CountAsync();
        var roles  = new List<string>();
        for (int i = 0; i < count; i++)
            roles.Add(await labels.Nth(i).InnerTextAsync());
        return roles;
    }

    public async Task ClickContinue()
    {
        await page.ClickAsync(ContinueButtonSelector);
        await page.WaitForURLAsync(url => url.Contains("check-answers"));
    }

    public async Task ClickCancel()
    {
        await page.ClickAsync(CancelLinkSelector);
        await page.WaitForURLAsync(url => url.Contains("user-assignments") && !url.Contains("assign"));
    }

    public async Task<bool> HasValidationError()
        => await page.IsVisibleAsync(ErrorSummarySelector);

    public async Task<string> GetPageUrl() => page.Url;
}
