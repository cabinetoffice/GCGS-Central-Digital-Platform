using System.Threading.Tasks;
using Microsoft.Playwright;

namespace E2ETests.Pages
{
    public class RegisteredAddressNonUkPage
    {
        private readonly IPage _page;

        // ✅ Page Locators
        private const string PageTitleSelector = "h1.govuk-fieldset__heading";
        private const string AddressLine1Input = "input#AddressLine1";
        private const string TownInput = "input#TownOrCity";
        private const string PostcodeInput = "input#Postcode";
        private const string CountrySelect = "select#Country";
        private const string SaveButtonSelector = "main >> button[type='submit']";

        public RegisteredAddressNonUkPage(IPage page)
        {
            _page = page;
        }

        public async Task<string> GetPageTitle()
        {
            await _page.WaitForSelectorAsync(PageTitleSelector);
            return await _page.InnerTextAsync(PageTitleSelector);
        }

        public async Task EnterAddress(string addressLine1, string townOrCity, string postcode, string countryCode)
        {
            await _page.FillAsync(AddressLine1Input, addressLine1);
            await _page.FillAsync(TownInput, townOrCity);
            await _page.FillAsync(PostcodeInput, postcode);
            await _page.SelectOptionAsync(CountrySelect, countryCode);
            Console.WriteLine($"✅ Filled postal address: {addressLine1}, {townOrCity}, {postcode}, {countryCode}");
        }

        public async Task ClickSave()
        {
            await _page.ClickAsync(SaveButtonSelector);
            Console.WriteLine("✅ Clicked 'Save' on Non-UK Registered Address Page");
        }

        public async Task CompletePage(string line1, string town, string postcode, string countryCode)
        {
            await EnterAddress(line1, town, postcode, countryCode);
            await ClickSave();
            Console.WriteLine("✅ Completed Non-UK Registered Address Page");
        }

        public async Task<bool> IsLoaded()
        {
            var title = await GetPageTitle();
            return title.Contains("Enter your organisation's registered address");
        }
    }
}