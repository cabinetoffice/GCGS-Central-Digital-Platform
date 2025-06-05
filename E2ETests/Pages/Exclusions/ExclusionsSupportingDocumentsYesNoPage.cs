using E2ETests.Utilities;
using Microsoft.Playwright;

namespace E2ETests.Pages.Exclusions;

public class ExclusionsSupportingDocumentsYesNoPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    private readonly string _formId = "0618b13e-eaf2-46e3-a7d2-6f2c44be7022";
    private readonly string _sectionId = "8a75cb04-fe29-45ae-90f9-168832dbea48";
    private readonly string _questionId = "d15651f9-b242-41ff-bb8e-b4aa6eec5a95";

    // ✅ Page Locators
    private readonly string PageTitleSelector = "h1.govuk-fieldset__heading";
    private readonly string ContinueButtonSelector = "button.govuk-button[type='submit']";
    private readonly string BackToSupplierInfoSelector = "a.govuk-back-link";
    public ExclusionsSupportingDocumentsYesNoPage(IPage page)
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
        Console.WriteLine($"📌 Navigated to Do you have a supporting document to upload? Page: {url}");

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
            await _page.GetByLabel("Upload a file").SetInputFilesAsync(
                new FilePayload { Buffer = System.Text.Encoding.UTF8.GetBytes("test file") , MimeType = "text/plain", Name = "file.txt"}
                );
        }
        Console.WriteLine($"✅ Selected Option: {(isYes ? "Yes" : "No")}");
    }

    public async Task CompletePage(bool isYes)
    {
        await SelectOption(isYes);
        await ClickContinue();
    }
}