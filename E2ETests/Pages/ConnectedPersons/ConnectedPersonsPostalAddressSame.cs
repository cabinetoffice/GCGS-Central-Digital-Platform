using System.Threading.Tasks;
using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages
{
    public class ConnectedPersonsPostalAddressSamePage
    {
        private readonly IPage _page;
        private readonly string _baseUrl;

        // ✅ Page Locators
        private readonly string PageTitleSelector = "h1.govuk-heading-l";
        private readonly string YesRadioButtonSelector = "input[name='postalAddressSame'][value='Yes']";
        private readonly string NoRadioButtonSelector = "input[name='postalAddressSame'][value='No']";
        private readonly string ContinueButtonSelector = "button.govuk-button[type='submit']";

        public ConnectedPersonsPostalAddressSamePage(IPage page)
        {
            _page = page;
            _baseUrl = ConfigUtility.GetBaseUrl(); // Retrieve base URL from ConfigUtility
        }

        public async Task NavigateTo(string storageKey)
        {
            string organisationId = StorageUtility.Retrieve(storageKey);
            if (string.IsNullOrEmpty(organisationId))
            {
                throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
            }

            string url = $"{_baseUrl}/organisation/{organisationId}/supplier-information/connected-person/postal-address-same-as-registered";
            await _page.GotoAsync(url);
            Console.WriteLine($"📌 Navigated to Connected Persons Postal Address Same Page: {url}");

            await _page.WaitForSelectorAsync(PageTitleSelector);
        }

        public async Task<string> GetPageTitle()
        {
            await _page.WaitForSelectorAsync(PageTitleSelector);
            return await _page.InnerTextAsync(PageTitleSelector);
        }

        public async Task SelectPostalAddressOption(bool isSame)
        {
            if (isSame)
            {
                await _page.ClickAsync(YesRadioButtonSelector);
                Console.WriteLine("✅ Selected: Yes - Postal address is the same as registered.");
            }
            else
            {
                await _page.ClickAsync(NoRadioButtonSelector);
                Console.WriteLine("✅ Selected: No - Postal address is different.");
            }
        }

        public async Task ClickContinue()
        {
            await _page.ClickAsync(ContinueButtonSelector);
        }

        /// Completes the form by selecting Yes/No for postal address and continuing.
        public async Task CompletePage(bool isSame)
        {
            await SelectPostalAddressOption(isSame);
            await ClickContinue();
            Console.WriteLine($"✅ Completed Postal Address Page with selection: {(isSame ? "Same as registered" : "Different")}");
        }
    }
}
