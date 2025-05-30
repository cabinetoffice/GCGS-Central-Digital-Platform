using System.Threading.Tasks;
using Microsoft.Playwright;

namespace E2ETests.Pages
{
    public class RegisteredAddressPage
    {
        private readonly IPage _page;

        // ✅ Page Locators
        private const string PageTitleSelector = "h1.govuk-fieldset__heading";
        private const string AddressLine1Input = "#AddressLine1";
        private const string TownOrCityInput = "#TownOrCity";
        private const string PostcodeInput = "#Postcode";
        private const string SaveButtonSelector = "main >> button[type='submit']";
        private const string NonUkAddressLink = "a[href*='registered-address/non-uk']";

        public RegisteredAddressPage(IPage page)
        {
            _page = page;
        }

        public async Task<string> GetPageTitle()
        {
            await _page.WaitForSelectorAsync(PageTitleSelector);
            return await _page.InnerTextAsync(PageTitleSelector);
        }

        public async Task FillInAddress(string line1, string town, string postcode)
        {
            await _page.FillAsync(AddressLine1Input, line1);
            await _page.FillAsync(TownOrCityInput, town);
            await _page.FillAsync(PostcodeInput, postcode);
            Console.WriteLine($"✅ Filled in registered address: {line1}, {town}, {postcode}");
        }

        public async Task ClickSave()
        {
            await _page.ClickAsync(SaveButtonSelector);
            Console.WriteLine("✅ Clicked 'Save' on Registered Address Page");
        }

        public async Task CompletePage(string line1, string town, string postcode)
        {
            await FillInAddress(line1, town, postcode);
            await ClickSave();
            Console.WriteLine("✅ Completed Registered Address Page");
        }

        public async Task ClickEnterNonUkAddress()
        {
            await _page.ClickAsync(NonUkAddressLink);
            Console.WriteLine("🌍 Clicked link to enter non-UK address");
        }

        public async Task<bool> IsLoaded()
        {
            var title = await GetPageTitle();
            return title.Contains("Enter your organisation's registered address");
        }
    }
}
