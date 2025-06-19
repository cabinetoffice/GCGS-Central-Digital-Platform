using Microsoft.Playwright;

namespace E2ETests.Pages;

public class DateOrganisationRegisteredPage
{
    private readonly IPage _page;

    // ✅ Selectors
    private const string PageHeadingSelector = "h1.govuk-heading-l";
    private const string DayInputSelector = "#registrationDateDay";
    private const string MonthInputSelector = "#registrationDateMonth";
    private const string YearInputSelector = "#registrationDateYear";
    private const string SaveButtonSelector = "main >> button[type='submit']";

    public DateOrganisationRegisteredPage(IPage page)
    {
        _page = page;
    }

    public async Task<string> GetPageHeading()
    {
        await _page.WaitForSelectorAsync(PageHeadingSelector);
        return await _page.InnerTextAsync(PageHeadingSelector);
    }

    public async Task EnterRegistrationDate(string day, string month, string year)
    {
        await _page.FillAsync(DayInputSelector, day);
        await _page.FillAsync(MonthInputSelector, month);
        await _page.FillAsync(YearInputSelector, year);
        System.Console.WriteLine($"✅ Entered registration date: {day}/{month}/{year}");
    }

    public async Task ClickSave()
    {
        await _page.ClickAsync(SaveButtonSelector);
        System.Console.WriteLine("✅ Clicked 'Save' on Date Organisation Registered Page");
    }

    public async Task CompletePage(string day, string month, string year)
    {
        await EnterRegistrationDate(day, month, year);
        await ClickSave();
        System.Console.WriteLine("✅ Completed Date Organisation Registered Page");
    }
}