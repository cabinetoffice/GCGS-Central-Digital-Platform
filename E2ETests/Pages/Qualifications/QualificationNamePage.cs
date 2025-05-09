using E2ETests.Utilities;
using Microsoft.Playwright;

namespace E2ETests.Pages.Qualifications;

public class QualificationNamePage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    private readonly string _formId = "0618b13e-eaf2-46e3-a7d2-6f2c44be7022";
    private readonly string _sectionId = "798cf1c1-40be-4e49-9adb-252219d5599d";
    private readonly string _questionId = "c8f36f11-b4fd-4dfd-9455-bbea0622b2ea";

    private readonly string PageTitleSelector = "#main-content > div > div.govuk-grid-column-full > h1";//*[@id=\"main-content\"]/div/div[2]/h1";
    private readonly string ContinueButtonSelector = "button:text('Continue')";
    private readonly string BackToSupplierInfoSelector = "a.govuk-back-link";
    private readonly string InputSelector = "input[name='TextInput'][type='text']";

    public QualificationNamePage(IPage page)
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

        string url = $"{_baseUrl}/organisation/{organisationId}/forms/{_formId}/sections/{_sectionId}/questions/{_questionId}";

        await _page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to Enter the qualification name page: {url}");

        await _page.WaitForSelectorAsync(PageTitleSelector);
    }

    public async Task<string> GetPageTitle()
    {
        await _page.WaitForSelectorAsync(PageTitleSelector);
        return await _page.InnerTextAsync(PageTitleSelector);
    }
    public async Task EnterQualificationName()
    {
        var qalificationName = "ISO 45001 Health and Safety Management";
        await _page.FillAsync(InputSelector, qalificationName);
        Console.WriteLine($"✅ Entered Qualification Name: {qalificationName}");
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
        await EnterQualificationName();
        await ClickContinue();
    }
}