using System.Threading.Tasks;
using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages
{
    public class TradeAssurancesWhenAwardedPage
    {
        private readonly IPage _page;
        private readonly string _baseUrl;

        // ✅ Page Locators
        private readonly string PageTitleSelector = "h1.govuk-fieldset__heading";
        private readonly string DayInputSelector = "#Day";
        private readonly string MonthInputSelector = "#Month";
        private readonly string YearInputSelector = "#Year";
        private readonly string ContinueButtonSelector = "button:text('Continue')";

        public TradeAssurancesWhenAwardedPage(IPage page)
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

            // Assume formId, sectionId and questionId are fixed
            string formId = "0618b13e-eaf2-46e3-a7d2-6f2c44be7022";
            string sectionId = "cf08acf8-e2fa-40c8-83e7-50c8671c343f";
            string questionId = "cc9da571-07d6-4926-9fd5-3fa543e2416b";

            string url = $"{_baseUrl}/organisation/{organisationId}/forms/{formId}/sections/{sectionId}/question/{questionId}";
            await _page.GotoAsync(url);
            Console.WriteLine($"📌 Navigated to Trade Assurances Know Reference Number Page: {url}");

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
            Console.WriteLine("✅ Completed Date Awarded Page.");
        }
    }
}
