using System.Threading.Tasks;
using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages
{
    public class ConnectedPersonsOrganisationRegisteredAddressPage
    {
        private readonly IPage _page;
        private readonly string _baseUrl;

        // ✅ Page Locators
        private readonly string PageTitleSelector = "h1.govuk-heading-l";
        private readonly string AddressLine1Selector = "input[name='addressLine1']";
        private readonly string TownCitySelector = "input[name='townCity']";
        private readonly string PostcodeSelector = "input[name='postcode']";
        private readonly string ContinueButtonSelector = "button.govuk-button[type='submit']";

        public ConnectedPersonsOrganisationRegisteredAddressPage(IPage page)
        {
            _page = page;
            _baseUrl = ConfigUtility.GetBaseUrl(); // Retrieve base URL from ConfigUtility
        }

        /// Navigates to the Connected Person Organisation Registered Address Page using stored Organisation ID.
        public async Task NavigateTo(string storageKey)
        {
            string organisationId = StorageUtility.Retrieve(storageKey);
            if (string.IsNullOrEmpty(organisationId))
            {
                throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
            }

            string url = $"{_baseUrl}/organisation/{organisationId}/supplier-information/connected-person/Registered-address/uk";
            await _page.GotoAsync(url);
            Console.WriteLine($"📌 Navigated to Connected Persons Organisation Registered Address Page: {url}");

            await _page.WaitForSelectorAsync(PageTitleSelector);
        }

        /// Retrieves the page title to verify the correct page is loaded.
        public async Task<string> GetPageTitle()
        {
            await _page.WaitForSelectorAsync(PageTitleSelector);
            return await _page.InnerTextAsync(PageTitleSelector);
        }

        /// Fills in the address fields.
        public async Task FillAddressFields(string addressLine1, string townCity, string postcode)
        {
            await _page.FillAsync(AddressLine1Selector, addressLine1);
            await _page.FillAsync(TownCitySelector, townCity);
            await _page.FillAsync(PostcodeSelector, postcode);

            Console.WriteLine($"✅ Address Fields Filled: {addressLine1}, {townCity}, {postcode}");
        }

        /// Clicks the 'Continue' button.
        public async Task ClickContinue()
        {
            await _page.ClickAsync(ContinueButtonSelector);
        }

        /// Completes the form by filling address details and continuing.
        public async Task CompletePage(string addressLine1, string city, string postcode)
        {
            await FillAddressFields(addressLine1, city, postcode);
            await ClickContinue();
            Console.WriteLine($"✅ Completed Organisation Registered Address Page with Address: {addressLine1}, {city}, {postcode}");
        }
    }
}
