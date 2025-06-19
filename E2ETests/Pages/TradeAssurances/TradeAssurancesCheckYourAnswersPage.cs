using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages;

public class TradeAssurancesCheckYourAnswersPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    // ✅ Page Locators
    private readonly string PageTitleSelector = "h1.govuk-heading-l";
    private readonly string AwardedByChangeLink = "a[href*='questions/179d597c-3db2-41de-af9b-c651e64d486d']";
    private readonly string ReferenceNumberChangeLink = "a[href*='questions/385cc8b6-5410-4f1e-95be-b43abb642797']";
    private readonly string DateAwardedChangeLink = "a[href*='questions/cc9da571-07d6-4926-9fd5-3fa543e2416b']";
    private readonly string SubmitButtonSelector = "button:text('Save')";

    public TradeAssurancesCheckYourAnswersPage(IPage page)
    {
        _page = page;
        _baseUrl = ConfigUtility.GetBaseUrl();
    }

    /// Navigates to the Trade Assurances Check Your Answers Page using stored Organisation ID.
    public async Task NavigateTo(string storageKey)
    {
        string organisationId = StorageUtility.Retrieve(storageKey);
        if (string.IsNullOrEmpty(organisationId))
        {
            throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
        }

        // Assume formId and sectionId are fixed
        string formId = "0618b13e-eaf2-46e3-a7d2-6f2c44be7022";
        string sectionId = "cf08acf8-e2fa-40c8-83e7-50c8671c343f";
        string questionId = "2c1c398d-1a97-4ce4-b06e-0cbf42e34ca8";

        string url = $"{_baseUrl}/organisation/{organisationId}/forms/{formId}/sections/{sectionId}/questions/{questionId}";
        await _page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to Trade Assurances Check Your Answers Page: {url}");

        await _page.WaitForSelectorAsync(PageTitleSelector);
    }

    public async Task<string> GetPageTitle()
    {
        await _page.WaitForSelectorAsync(PageTitleSelector);
        return await _page.InnerTextAsync(PageTitleSelector);
    }

    public async Task ClickChangeAwardedBy()
    {
        await _page.ClickAsync(AwardedByChangeLink);
        Console.WriteLine("✅ Clicked 'Change' for Awarded By");
    }

    public async Task ClickChangeReferenceNumber()
    {
        await _page.ClickAsync(ReferenceNumberChangeLink);
        Console.WriteLine("✅ Clicked 'Change' for Reference Number");
    }

    public async Task ClickChangeDateAwarded()
    {
        await _page.ClickAsync(DateAwardedChangeLink);
        Console.WriteLine("✅ Clicked 'Change' for Date Awarded");
    }

    public async Task ClickSubmit()
    {
        await _page.ClickAsync(SubmitButtonSelector);
        Console.WriteLine("✅ Clicked 'Save' on Check Your Answers Page");
    }

    public async Task CompletePage()
    {
        await _page.PauseAsync();
        await ClickSubmit();
        Console.WriteLine($"✅ Submitted Trade Assurances Check Your Answers Page");
    }

    public async Task<bool> IsLoaded()
    {
        return await _page.Locator(PageTitleSelector).InnerTextAsync() == "Check your answers";
    }
}
