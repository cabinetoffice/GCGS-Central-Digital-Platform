using Microsoft.Playwright;

namespace E2ETests.Pages;

public class TradeAssurancesSummaryPage
{
    private readonly IPage _page;

    // ✅ Page Locators

    private readonly string ChangeLinkSelector = "a:text('Change')";
    private readonly string RemoveLinkSelector = "a:text('Remove')";

    private readonly string YesRadioSelector = "input[name='AddAnotherAnswerSet'][value='true']";
    private readonly string NoRadioSelector = "input[name='AddAnotherAnswerSet'][value='false']";

    private readonly string ContinueButtonSelector = "button:text('Continue')";

    public TradeAssurancesSummaryPage(IPage page)
    {
        _page = page;
    }

    public async Task ClickChange()
    {
        await _page.ClickAsync(ChangeLinkSelector);
    }

    public async Task ClickRemove()
    {
        await _page.ClickAsync(RemoveLinkSelector);
    }

    public async Task SelectAddAnotherTradeAssurance(bool addAnother)
    {
        string radioButton = addAnother ? YesRadioSelector : NoRadioSelector;
        await _page.ClickAsync(radioButton);
    }

    public async Task ClickContinue()
    {
        await _page.ClickAsync(ContinueButtonSelector);
    }

    public async Task ClickBack()
    {
        await _page.ClickAsync("a.govuk-back-link");
        Console.WriteLine("✅ Clicked 'Back' on Summary Page");
    }

    public async Task AssertTradeAssurancesCount(int expectedCount)
    {
        string expectedText = $"Trade assurances\n{expectedCount} added";
        string selector = "h1.govuk-heading-l";

        await _page.WaitForSelectorAsync(selector);
        string actualText = await _page.InnerTextAsync(selector);

        if (actualText.Trim() != expectedText)
        {
            throw new System.Exception($"❌ Expected '{expectedText}' but found '{actualText}'");
        }

        Console.WriteLine($"✅ Trade assurances count verified: {actualText}");
    }


    /// Completes the page by selecting Yes/No for adding another trade assurance and clicking Continue.
    public async Task CompletePage(bool addAnother)
    {
        await SelectAddAnotherTradeAssurance(addAnother);
        await ClickContinue();
        Console.WriteLine("✅ Completed Trade Assurances Summary Page");
    }
}