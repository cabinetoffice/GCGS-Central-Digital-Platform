using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages;

public class ConnectedPersonsDateRegisteredPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    // ✅ Page Locators
    private readonly string PageTitleSelector = "h1.govuk-fieldset__heading";
    private readonly string DayInputSelector = "#connectedPersonDateDay";
    private readonly string MonthInputSelector = "#connectedPersonDateMonth";
    private readonly string YearInputSelector = "#connectedPersonDateYear";
    private readonly string ContinueButtonSelector = "button.govuk-button[type='submit']";

    public ConnectedPersonsDateRegisteredPage(IPage page)
    {
        _page = page;
        _baseUrl = ConfigUtility.GetBaseUrl();
    }

    public async Task NavigateTo(string storageKey)
    {
        string organisationId = StorageUtility.Retrieve(storageKey);
        if (string.IsNullOrEmpty(organisationId))
        {
            throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
        }

        string url = $"{_baseUrl}/organisation/{organisationId}/supplier-information/connected-person/date-registered";
        await _page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to Connected Persons Date Registered Page: {url}");

        await _page.WaitForSelectorAsync(PageTitleSelector);
    }

    public async Task<string> GetPageTitle()
    {
        await _page.WaitForSelectorAsync(PageTitleSelector);
        return await _page.InnerTextAsync(PageTitleSelector);
    }

    public async Task EnterDate(string day, string month, string year)
    {
        await _page.FillAsync(DayInputSelector, day);
        await _page.FillAsync(MonthInputSelector, month);
        await _page.FillAsync(YearInputSelector, year);

        Console.WriteLine($"🗓️ Entered date: {day}-{month}-{year}");
    }

    public async Task ClickContinue()
    {
        await _page.ClickAsync(ContinueButtonSelector);
    }

    public async Task CompletePage(string day, string month, string year)
    {
        await EnterDate(day, month, year);
        await ClickContinue();
        Console.WriteLine("✅ Completed Date Registered Page.");
    }
}
