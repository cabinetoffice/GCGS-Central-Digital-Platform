using E2ETests.Utilities;
using Microsoft.Playwright;

namespace E2ETests.Pages.Exclusions;

public class ExclusionsDescribeInDetailsPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    private readonly string _formId = "0618b13e-eaf2-46e3-a7d2-6f2c44be7022";
    private readonly string _sectionId = "8a75cb04-fe29-45ae-90f9-168832dbea48";
    private readonly string _questionId = "fdb69829-85a9-406b-b984-7aeae58aeb00";

    // ✅ Page Locators
    private readonly string PageTitleSelector = "label.govuk-label--l";
    private readonly string ContinueButtonSelector = "button.govuk-button[type='submit']";
    private readonly string BackToSupplierInfoSelector = "a.govuk-back-link";
    private readonly string InputSelector = "#TextInput";
    public ExclusionsDescribeInDetailsPage(IPage page)
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
        Console.WriteLine($"📌 Navigated to Describe the exclusion in more detail Page: {url}");

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

    public async Task EnterExclusionInDetails()
    {
        var details = "Very unfair exclusion";
        await _page.FillAsync(InputSelector, details);
        Console.WriteLine($"✅ Entered exclusion in more detail: {details}");
    }

    public async Task CompletePage()
    {
        await EnterExclusionInDetails();
        await ClickContinue();
    }
}