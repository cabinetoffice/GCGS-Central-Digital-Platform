using E2ETests.Utilities;
using Microsoft.Playwright;

namespace E2ETests.Pages.Qualifications;

public class QualificationSummaryPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    private readonly string ChangeLinkSelector = "a:text('Change')";
    private readonly string RemoveLinkSelector = "a:text('Remove')";

    private readonly string YesRadioSelector = "input[name='AddAnotherAnswerSet'][value='true']";
    private readonly string NoRadioSelector = "input[name='AddAnotherAnswerSet'][value='false']";

    private readonly string ContinueButtonSelector = "button:text('Continue')";

    public QualificationSummaryPage(IPage page)
    {
        _page = page;
        _baseUrl = ConfigUtility.GetBaseUrl(); // Get base URL from configuration utility
    }    

    public async Task ClickChange()
    {
        await _page.ClickAsync(ChangeLinkSelector);
    }

    public async Task ClickRemove()
    {
        await _page.ClickAsync(RemoveLinkSelector);
    }
    public async Task SelectAddAnotherQualification(bool addAnother)
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
    public async Task AssertQualificationCount(int expectedCount)
    {
        //await _page.PauseAsync();

        string expectedText = $"Qualifications\n{expectedCount} added";

        string selector = "h1.govuk-heading-l";

        await _page.WaitForSelectorAsync(selector);
        string actualText = await _page.InnerTextAsync(selector);

        if (actualText.Trim() != expectedText)
        {
            throw new System.Exception($"❌ Expected '{expectedText}' but found '{actualText}'");
        }

        Console.WriteLine($"✅ Qualification count verified: {actualText}");
    }


    /// Completes the page by selecting Yes/No for adding another trade assurance and clicking Continue.
    public async Task CompletePage(bool addAnother)
    {
        await SelectAddAnotherQualification(addAnother);
        await ClickContinue();
        Console.WriteLine("✅ Completed Qualification Summary Page");
    }
}