using E2ETests.Utilities;
using Microsoft.Playwright;

namespace E2ETests.Pages.Exclusions;

public class ExclusionsDecisionRecordedYesNoPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    private readonly string _formId = "0618b13e-eaf2-46e3-a7d2-6f2c44be7022";
    private readonly string _sectionId = "8a75cb04-fe29-45ae-90f9-168832dbea48";
    private readonly string _questionId = "715b4bcb-a7e8-47bf-a50e-c54895e4b8e6";

    // ✅ Page Locators
    private readonly string PageTitleSelector = "h1.govuk-fieldset__heading";
    private readonly string ContinueButtonSelector = "button.govuk-button[type='submit']";
    private readonly string BackToSupplierInfoSelector = "a.govuk-back-link";
    private readonly string WebsiteAddressInputSelector = "#TextInput";
    public ExclusionsDecisionRecordedYesNoPage(IPage page)
    {
        _page = page;
        _baseUrl = ConfigUtility.GetBaseUrl(); // Retrieve base URL from ConfigUtility
    }

    public async Task NavigateTo(string storageKey)
    {
        string organisationId = StorageUtility.Retrieve(storageKey);
        if (string.IsNullOrEmpty(organisationId))
        {
            throw new Exception($"❌ Organisation ID not found for key: {storageKey}");
        }

        string url = $"{_baseUrl}/organisation/{organisationId}/forms/{_formId}/sections/{_sectionId}/questions/{_questionId}";

        await _page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to Was the decision recorded on a public authority website? Page: {url}");

        await _page.WaitForSelectorAsync(PageTitleSelector);
    }

    public async Task<string> GetPageTitle()
    {
        await _page.WaitForSelectorAsync(PageTitleSelector);
        return await _page.InnerTextAsync(PageTitleSelector);
    }

    public async Task ClickContinue()
    {
        await _page.ClickAsync(ContinueButtonSelector);
    }

    public async Task ClickBackToSupplierInformation()
    {
        await _page.ClickAsync(BackToSupplierInfoSelector);
    }
    public async Task SelectOption(bool isYes)
    {
        string radioSelector = isYes ? "Yes" : "No";
        await _page.GetByLabel(radioSelector).CheckAsync();
        if (isYes)
        {
            await _page.FillAsync(WebsiteAddressInputSelector, "https://www.example.gov.uk");
        }                
        Console.WriteLine($"✅ Selected Option: {(isYes ? "Yes" : "No")}");
    }

    public async Task CompletePage(bool isYes)
    {
        await SelectOption(isYes);
        await ClickContinue();
    }
}