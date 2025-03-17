using System.Threading.Tasks;
using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages
{
    public class ConnectPersonsSupplierHasControlPage
    {
        private readonly IPage _page;
        private readonly string _baseUrl;

        // ✅ Page Locators
        private readonly string PageTitleSelector = "h1.govuk-heading-l";
        private readonly string YesRadioSelector = "input[name='influence'][value='yes']";
        private readonly string NoRadioSelector = "input[name='influence'][value='no']";
        private readonly string ContinueButtonSelector = "button.govuk-button[type='submit']";

        public ConnectPersonsSupplierHasControlPage(IPage page)
        {
            _page = page;
            _baseUrl = ConfigUtility.GetBaseUrl(); // Retrieve base URL from ConfigUtility
        }

        /// Navigates to the Connected Person Influence page using stored Organisation ID.
        public async Task NavigateTo(string storageKey)
        {
            string organisationId = StorageUtility.Retrieve(storageKey);
            if (string.IsNullOrEmpty(organisationId))
            {
                throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
            }

            string url = $"{_baseUrl}/organisation/{organisationId}/supplier-information/connected-person/supplier-has-control";
            await _page.GotoAsync(url);
            Console.WriteLine($"📌 Navigated to Connected Person Influence Page: {url}");

            await _page.WaitForSelectorAsync(PageTitleSelector);
        }

        /// Retrieves the page title to verify the correct page is loaded.
        public async Task<string> GetPageTitle()
        {
            await _page.WaitForSelectorAsync(PageTitleSelector);
            return await _page.InnerTextAsync(PageTitleSelector);
        }

        /// Selects either "Yes" or "No" for the influence question.
        public async Task SelectInfluenceOption(bool isInfluenced)
        {
            string radioSelector = isInfluenced ? YesRadioSelector : NoRadioSelector;
            await _page.CheckAsync(radioSelector);
            Console.WriteLine($"✅ Selected Influence Option: {(isInfluenced ? "Yes" : "No")}");
        }

        /// Clicks the 'Continue' button.
        public async Task ClickContinue()
        {
            await _page.ClickAsync(ContinueButtonSelector);
        }

        /// Completes the form by selecting influence option and continuing.
        public async Task CompletePage(bool isInfluenced)
        {
            await SelectInfluenceOption(isInfluenced);
            await ClickContinue();
        }
    }
}
