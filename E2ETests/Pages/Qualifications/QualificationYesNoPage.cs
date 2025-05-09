using E2ETests.Utilities;
using Microsoft.Playwright;

namespace E2ETests.Pages.Qualifications;

public class QualificationYesNoPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    private readonly string _formId = "0618b13e-eaf2-46e3-a7d2-6f2c44be7022";
    private readonly string _sectionId = "798cf1c1-40be-4e49-9adb-252219d5599d";

    private readonly string PageTitleSelector = "h1.govuk-heading-l";
    private readonly string ContinueButtonSelector = "main >> button.govuk-button[type='submit']";
    private readonly string BackToSupplierInfoSelector = "a.govuk-back-link";
    private readonly string YesRadioSelector = "input[name='AddAnotherAnswerSet'][value='true']";
    private readonly string NoRadioSelector = "input[name='AddAnotherAnswerSet'][value='false']";

    public QualificationYesNoPage(IPage page)
    {
        _page = page;
        _baseUrl = ConfigUtility.GetBaseUrl(); // Get base URL from configuration utility
    }

    public async Task NavigateTo(string storageKey)
    {
        string organisationId = StorageUtility.Retrieve(storageKey);
        if (string.IsNullOrEmpty(organisationId))
        {
            throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
        }

        string url = $"{_baseUrl}/organisation/{organisationId}/forms/{_formId}/sections/{_sectionId}/summary";

        await _page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to Qualification Yes or No Page: {url}");

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