using E2ETests.Utilities;
using Microsoft.Playwright;

namespace E2ETests.Pages.Exclusions;

public class ExclusionSummaryPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;


    private readonly string _formId = "0618b13e-eaf2-46e3-a7d2-6f2c44be7022";
    private readonly string _sectionId = "8a75cb04-fe29-45ae-90f9-168832dbea48";

    private readonly string ViewLinkSelector = "a:text('View')";
    private readonly string RemoveLinkSelector = "a:text('Remove')";

    private readonly string PageTitleSelector = "h1.govuk-heading-l";
    private readonly string YesRadioSelector = "input[name='AddAnotherAnswerSet'][value='true']";
    private readonly string NoRadioSelector = "input[name='AddAnotherAnswerSet'][value='false']";

    private readonly string ContinueButtonSelector = "button:text('Continue')";

    public ExclusionSummaryPage(IPage page)
    {
        _page = page;
        _baseUrl = ConfigUtility.GetBaseUrl(); // Get base URL from configuration utility
    }

    public async Task NavigateTo(string storageKey)
    {
        string organisationId = StorageUtility.Retrieve(storageKey);
        if (string.IsNullOrEmpty(organisationId))
        {
            throw new Exception($"❌ Organisation ID not found for key: {storageKey}");
        }

        string url = $"{_baseUrl}/organisation/{organisationId}/forms/{_formId}/sections/{_sectionId}/summary";

        await _page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to Exclusions Summary Page: {url}");

        await _page.WaitForSelectorAsync(PageTitleSelector);
    }

    public async Task ClickView()
    {
        await _page.ClickAsync(ViewLinkSelector);
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

    public async Task AssertExclusionsCount(int expectedCount)
    {        
        string expectedText = $"Exclusions\n{expectedCount} added";

        await _page.WaitForSelectorAsync(PageTitleSelector);
        string actualText = await _page.InnerTextAsync(PageTitleSelector);

        if (actualText.Trim() != expectedText)
        {
            throw new System.Exception($"❌ Expected '{expectedText}' but found '{actualText}'");
        }
        Console.WriteLine($"✅ Exclusions count verified: {actualText}");
        await _page.PauseAsync();
    }

    public async Task CompletePage(bool addAnother)
    {
        await SelectAddAnotherQualification(addAnother);
        await ClickContinue();
        Console.WriteLine("✅ Completed Exclusions Summary Page");
    }
}