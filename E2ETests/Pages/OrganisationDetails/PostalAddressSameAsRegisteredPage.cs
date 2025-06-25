using Microsoft.Playwright;

namespace E2ETests.Pages;

public class PostalAddressSameAsRegisteredPage
{
    private readonly IPage _page;

    // ✅ Page Locators
    private const string PageTitleSelector = "h1.govuk-heading-l";
    private const string YesRadioSelector = "input#YES";
    private const string NoRadioSelector = "input#NO";
    private const string ContinueButtonSelector = "main >> button[type='submit']";

    public PostalAddressSameAsRegisteredPage(IPage page)
    {
        _page = page;
    }

    public async Task<string> GetPageTitle()
    {
        await _page.WaitForSelectorAsync(PageTitleSelector);
        return await _page.InnerTextAsync(PageTitleSelector);
    }

    public async Task SelectDifferentFromRegistered(bool isDifferent)
    {
        var selector = isDifferent ? YesRadioSelector : NoRadioSelector;
        await _page.ClickAsync(selector);
        Console.WriteLine($"✅ Selected postal address different from registered: {isDifferent}");
    }

    public async Task ClickContinue()
    {
        await _page.ClickAsync(ContinueButtonSelector);
        Console.WriteLine("✅ Clicked 'Continue' on Postal Address Same As Registered Page");
    }

    public async Task CompletePage(bool isDifferentFromRegistered)
    {
        await SelectDifferentFromRegistered(isDifferentFromRegistered);
        await ClickContinue();
        Console.WriteLine("✅ Completed Postal Address Same As Registered Page");
    }

    public async Task<bool> IsLoaded()
    {
        var title = await GetPageTitle();
        return title.Contains("Does your organisation have a different postal address?");
    }
}