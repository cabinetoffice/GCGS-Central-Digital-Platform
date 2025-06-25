using E2ETests.Utilities;
using Microsoft.Playwright;

namespace E2ETests.Pages.Exclusions;

public class SelectExclusionAppliesToPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    private readonly string _formId = "0618b13e-eaf2-46e3-a7d2-6f2c44be7022";
    private readonly string _sectionId = "8a75cb04-fe29-45ae-90f9-168832dbea48";
    private readonly string _questionId = "c4e24140-e0c3-4ab8-aea1-c5807185906b";

    // ✅ Page Locators
    private readonly string PageTitleSelector = "h1.govuk-fieldset__heading";
    private readonly string BackToSupplierInfoSelector = "a.govuk-back-link";
    private readonly string RadioSelector = "input#SelectedOption";

    public SelectExclusionAppliesToPage(IPage page)
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
        Console.WriteLine($"📌 Navigated to Select who the exclusion applies to Page: {url}");

        await _page.WaitForSelectorAsync(PageTitleSelector);
    }

    public async Task<string> GetPageTitle()
    {
        await _page.WaitForSelectorAsync(PageTitleSelector);
        return await _page.InnerTextAsync(PageTitleSelector);
    }

    public async Task ClickContinue()
    {
        await _page.GetByRole(AriaRole.Button, new() { Name = "Continue" }).ClickAsync();
    }

    public async Task ClickBackToSupplierInformation()
    {
        await _page.ClickAsync(BackToSupplierInfoSelector);
    }
    public async Task SelectOption()
    {
        await _page.CheckAsync(RadioSelector);
        Console.WriteLine($"✅ Selected Option: 1st supplier from the list");
    }

    public async Task CompletePage()
    {
        await SelectOption();
        await ClickContinue();
    }
}