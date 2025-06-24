using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages;

public class TradeAssurancesYesNoPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    // ✅ Page Locators
    private readonly string PageTitleSelector = "h1.govuk-fieldset__heading";
    private readonly string ContinueButtonSelector = "main >> button.govuk-button[type='submit']";
    private readonly string BackToSupplierInfoSelector = "a.govuk-back-link";
    private readonly string YesRadioSelector = "input[name='Confirm'][value='true']";
    private readonly string NoRadioSelector = "input[name='Confirm'][value='false']";

    public TradeAssurancesYesNoPage(IPage page)
    {
        _page = page;
        _baseUrl = ConfigUtility.GetBaseUrl(); // Retrieve base URL from ConfigUtility
    }

    /// Navigates to the Trade Assurances Yes/No page using stored Organisation ID.
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

        string url = $"{_baseUrl}/organisation/{organisationId}/forms/{formId}/sections/{sectionId}/check-further-questions-exempted";
        await _page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to Trade Assurances Yes/No Page: {url}");

        await _page.WaitForSelectorAsync(PageTitleSelector);
    }

    public async Task<string> GetPageTitle()
    {
        await _page.WaitForSelectorAsync(PageTitleSelector);
        return await _page.InnerTextAsync(PageTitleSelector);
    }

    public async Task SelectOption(bool isYes)
    {
        string radioSelector = isYes ? YesRadioSelector : NoRadioSelector;
        await _page.CheckAsync(radioSelector);
        Console.WriteLine($"✅ Selected Option: {(isYes ? "Yes" : "No")}");
    }

    public async Task ClickContinue()
    {
        await _page.ClickAsync(ContinueButtonSelector);
    }

    public async Task ClickBackToSupplierInformation()
    {
        await _page.ClickAsync(BackToSupplierInfoSelector);
    }

    public async Task CompletePage(bool isYes)
    {
        await SelectOption(isYes);
        await ClickContinue();
    }
}
