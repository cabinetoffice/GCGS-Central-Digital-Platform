using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages;

public class TradeAssurancesKnowReferenceNumberPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    // ✅ Page Locators
    private readonly string PageTitleSelector = "h1.govuk-fieldset__heading";
    private readonly string ContinueButtonSelector = "button:text('Continue')";
    private readonly string BackToSupplierInfoSelector = "a.govuk-back-link";
    private readonly string YesRadioSelector = "input[type='radio'][aria-controls='conditional-YES']";
    private readonly string NoRadioSelector = "input[type='radio'][aria-controls='conditional-NO']";
    private readonly string InputSelector = "input[type='text'][name='TextInput']";

    public TradeAssurancesKnowReferenceNumberPage(IPage page)
    {
        _page = page;
        _baseUrl = ConfigUtility.GetBaseUrl(); // Retrieve base URL from ConfigUtility
    }

    /// Navigates to the Trade Assurances Know Reference Number page using stored Organisation ID.
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
        string questionId = "385cc8b6-5410-4f1e-95be-b43abb642797";

        string url = $"{_baseUrl}/organisation/{organisationId}/forms/{formId}/sections/{sectionId}/question/{questionId}";
        await _page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to Trade Assurances Know Reference Number Page: {url}");

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


    public async Task InputReferenceNumber()
    {
        await _page.FillAsync(InputSelector, "123456789");
    }

    public async Task ClickContinue()
    {
        await _page.ClickAsync(ContinueButtonSelector);

    }

    public async Task ClickBackToSupplierInformation()
    {
        await _page.ClickAsync(BackToSupplierInfoSelector);
    }

    public async Task CompletePage()
    {
        await SelectOption(true);
        await InputReferenceNumber();
        await ClickContinue();
    }

}
