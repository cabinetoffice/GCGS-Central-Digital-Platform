using E2ETests.Utilities;
using Microsoft.Playwright;

namespace E2ETests.Pages.Qualifications;

public class QualificationWhenAwardedPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    private readonly string _formId = "0618b13e-eaf2-46e3-a7d2-6f2c44be7022";
    private readonly string _sectionId = "798cf1c1-40be-4e49-9adb-252219d5599d";
    private readonly string _questionId = "df2587ef-7bbf-4fe8-8e19-b63526254380";

    private readonly string PageTitleSelector = "h1.govuk-label--l";
    private readonly string ContinueButtonSelector = "button:text('Continue')";
    private readonly string BackLinkSelector = "a.govuk-back-link";
    private readonly string DayInputSelector = "#Day";
    private readonly string MonthInputSelector = "#Month";
    private readonly string YearInputSelector = "#Year";

    public QualificationWhenAwardedPage(IPage page)
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

        string url = $"{_baseUrl}/organisation/{organisationId}/forms/{_formId}/sections/{_sectionId}/questions/{_questionId}";

        await _page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to What date was the qualification awarded? page: {url}");

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

    public async Task ClickBackLink()
    {
        await _page.ClickAsync(BackLinkSelector);
    }
    public async Task CompletePage(string day, string month, string year)
    {
        await EnterDate(day, month, year);
        await ClickContinue();
        Console.WriteLine("✅ Completed Date the qualification awarded Page.");
    }
}