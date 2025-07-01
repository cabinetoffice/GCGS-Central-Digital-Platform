using E2ETests.Utilities;
using Microsoft.Playwright;

namespace E2ETests.Pages.Exclusions;

public class ExclusionsEndedYesNoPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    private readonly string _formId = "0618b13e-eaf2-46e3-a7d2-6f2c44be7022";
    private readonly string _sectionId = "8a75cb04-fe29-45ae-90f9-168832dbea48";
    private readonly string _questionId = "76c775e7-717f-4938-9d65-af67b54df113";

    // ✅ Page Locators
    private readonly string PageTitleSelector = "h1.govuk-fieldset__heading";
    private readonly string BackToSupplierInfoSelector = "a.govuk-back-link";
    private readonly string DayInputSelector = "#Day";
    private readonly string MonthInputSelector = "#Month";
    private readonly string YearInputSelector = "#Year";
    public ExclusionsEndedYesNoPage(IPage page)
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
        Console.WriteLine($"📌 Navigated to Have the circumstances that led to the exclusion ended? Page: {url}");

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
    public async Task SelectOption(bool isYes)
    {
        string radioSelector = isYes ? "Yes" : "No";
        await _page.GetByLabel(radioSelector).CheckAsync();

        if (isYes)
        {
            await _page.FillAsync(DayInputSelector, "1");
            await _page.FillAsync(MonthInputSelector, "8");
            await _page.FillAsync(YearInputSelector, "2015");
            Console.WriteLine($"🗓️ Entered date: 1-8-2015");
        }
              
        Console.WriteLine($"✅ Selected Option exclusion ended? : {(isYes ? "Yes" : "No")}");
    }

    public async Task CompletePage(bool isYes)
    {
        await SelectOption(isYes);
        await ClickContinue();
    }
}