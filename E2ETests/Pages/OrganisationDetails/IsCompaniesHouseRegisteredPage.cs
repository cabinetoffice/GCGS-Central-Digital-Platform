using System.Threading.Tasks;
using Microsoft.Playwright;

namespace E2ETests.Pages
{
    public class IsCompaniesHouseRegisteredPage
    {
        private readonly IPage _page;

        // ✅ Selectors
        private const string PageHeadingSelector = "h1.govuk-fieldset__heading";
        private const string YesRadioSelector = "input#regOnCh";
        private const string NoRadioSelector = "input#regOnCh-2";
        private const string ContinueButtonSelector = "main >> button[type='submit']";

        public IsCompaniesHouseRegisteredPage(IPage page)
        {
            _page = page;
        }

        public async Task<string> GetPageHeading()
        {
            await _page.WaitForSelectorAsync(PageHeadingSelector);
            return await _page.InnerTextAsync(PageHeadingSelector);
        }

        public async Task SelectAnswer(bool isRegistered)
        {
            string selector = isRegistered ? YesRadioSelector : NoRadioSelector;
            await _page.CheckAsync(selector);
            System.Console.WriteLine($"✅ Selected Companies House registration: {(isRegistered ? "Yes" : "No")}");
        }

        public async Task ClickContinue()
        {
            await _page.ClickAsync(ContinueButtonSelector);
            System.Console.WriteLine("✅ Clicked 'Continue' on Companies House Registration Page");
        }

        public async Task CompletePage(bool isRegistered)
        {
            await SelectAnswer(isRegistered);
            await ClickContinue();
            System.Console.WriteLine("✅ Completed Companies House Registration Page");
        }
    }
}