using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages;

public class UserSummaryPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    // ✅ Page Locators
    private readonly string PageHeadingSelector = "h2.govuk-heading-l";
    private readonly string UserSummaryListSelector = "dl.govuk-summary-list";
    private readonly string UserEmailSelector = "dd.govuk-summary-list__value strong";
    private readonly string AddAnotherUserYesSelector = "input[name='HasPerson'][value='true']";
    private readonly string AddAnotherUserNoSelector = "input[name='HasPerson'][value='false']";
    private readonly string ContinueButtonSelector = "button:has-text(\"Continue\")";
    private readonly string BackLinkSelector = "a.govuk-back-link";

    public UserSummaryPage(IPage page)
    {
        _page = page;
        _baseUrl = ConfigUtility.GetBaseUrl();
    }

    /// Navigates to the User Summary page using stored Organisation ID.
    public async Task NavigateTo(string storageKey)
    {
        string organisationId = StorageUtility.Retrieve(storageKey);
        if (string.IsNullOrEmpty(organisationId))
        {
            throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
        }

        string url = $"{_baseUrl}/organisation/{organisationId}/users/user-summary";
        await _page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to User Summary Page: {url}");

        await _page.WaitForSelectorAsync(PageHeadingSelector);
    }

    public async Task<string> GetPageHeading()
    {
        return await _page.InnerTextAsync(PageHeadingSelector);
    }

    public async Task<string> GetDisplayedUserEmail()
    {
        return await _page.InnerTextAsync(UserEmailSelector);
    }

    public async Task SelectAddAnotherUser(bool addAnother)
    {
        string selector = addAnother ? AddAnotherUserYesSelector : AddAnotherUserNoSelector;
        await _page.ClickAsync(selector);
    }

    public async Task ClickContinue()
    {
        await _page.ClickAsync(ContinueButtonSelector);
        Console.WriteLine("✅ Clicked 'Continue' on User Summary Page");
    }

    public async Task ClickBack()
    {
        await _page.ClickAsync(BackLinkSelector);
        Console.WriteLine("✅ Clicked 'Back' on User Summary Page");
    }

    /// Completes the page by choosing whether to add another user and clicking Continue
    public async Task CompletePage(bool addAnotherUser)
    {
        await SelectAddAnotherUser(addAnotherUser);
        await ClickContinue();
        Console.WriteLine("✅ Completed User Summary Page");
    }

    /// Check if a specific user is listed by full name and email
    public async Task<bool> AssertUserListed(string fullName, string email)
    {
        var summaryLists = _page.Locator(UserSummaryListSelector);
        int count = await summaryLists.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var text = await summaryLists.Nth(i).InnerTextAsync();
            if (text.Contains(fullName) && text.Contains(email))
                return true;
        }
        return false;
    }

    public async Task AssertUserCount(int expectedCount)
    {
        string expectedText = expectedCount == 1 ? $"Organisation has {expectedCount} user" : $"Organisation has {expectedCount} users";

        await _page.WaitForSelectorAsync(PageHeadingSelector);
        string actualText = await _page.InnerTextAsync(PageHeadingSelector);

        if (actualText.Trim() != expectedText)
        {
            throw new System.Exception($"❌ Expected '{expectedText}' but found '{actualText}'");
        }

        Console.WriteLine($"✅ User count verified: {actualText}");
    }


}
