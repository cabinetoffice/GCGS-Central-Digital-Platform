using Microsoft.Playwright;

namespace E2ETests.Pages.Applications;

/// <summary>
/// Page Object Model for /organisation/{id}/applications/{appId}/user-assignments/{userId}/revoke
/// </summary>
public class RevokeConfirmationPage(IPage page)
{
    private const string HeadingSelector       = "h1.govuk-heading-l";
    private const string ConfirmYesSelector    = "input#confirm-yes";
    private const string ConfirmNoSelector     = "input#confirm-no";
    private const string ContinueButtonSelector = "button:has-text('Continue')";

    public async Task<string> GetHeading()
        => await page.InnerTextAsync(HeadingSelector);

    public async Task SelectYes()
        => await page.CheckAsync(ConfirmYesSelector);

    public async Task SelectNo()
        => await page.CheckAsync(ConfirmNoSelector);

    public async Task ClickContinue()
    {
        await page.ClickAsync(ContinueButtonSelector);
        await page.WaitForURLAsync(url => url.Contains("user-assignments") && !url.Contains("revoke"));
    }

    public async Task ConfirmRevoke()
    {
        await SelectYes();
        await ClickContinue();
    }

    public async Task<string> GetPageUrl() => page.Url;
}
