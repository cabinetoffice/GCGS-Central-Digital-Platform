using System.Threading.Tasks;
using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages
{
    public class ConnectedPersonPersonTypePage
    {
        private readonly IPage _page;
        private readonly string _baseUrl;

        // ✅ Page Locators
        private readonly string PageTitleSelector = "h1.govuk-heading-l";
        private readonly string OrganisationRadioSelector = "input[name='personType'][value='organisation']";
        private readonly string IndividualRadioSelector = "input[name='personType'][value='individual']";
        private readonly string TrustRadioSelector = "input[name='personType'][value='trust']";
        private readonly string ContinueButtonSelector = "button.govuk-button[type='submit']";

        public ConnectedPersonPersonTypePage(IPage page)
        {
            _page = page;
            _baseUrl = ConfigUtility.GetBaseUrl(); // Retrieve base URL from ConfigUtility
        }

        /// Navigates to the Connected Person Type selection page using stored Organisation ID.
        public async Task NavigateTo(string storageKey)
        {
            string organisationId = StorageUtility.Retrieve(storageKey);
            if (string.IsNullOrEmpty(organisationId))
            {
                throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
            }

            string url = $"{_baseUrl}/organisation/{organisationId}/supplier-information/connected-person/person-type";
            await _page.GotoAsync(url);
            Console.WriteLine($"📌 Navigated to Connected Person Type Page: {url}");

            await _page.WaitForSelectorAsync(PageTitleSelector);
        }

        /// Retrieves the page title to verify the correct page is loaded.
        public async Task<string> GetPageTitle()
        {
            await _page.WaitForSelectorAsync(PageTitleSelector);
            return await _page.InnerTextAsync(PageTitleSelector);
        }

        /// Selects a person type option from the available choices.
        public async Task SelectPersonType(string personType)
        {
            string radioSelector = personType.ToLower() switch
            {
                "organisation" => OrganisationRadioSelector,
                "individual" => IndividualRadioSelector,
                "trust" => TrustRadioSelector,
                _ => throw new System.Exception($"❌ Invalid person type: {personType}. Must be 'organisation', 'individual', or 'trust'.")
            };

            await _page.CheckAsync(radioSelector);
            Console.WriteLine($"✅ Selected Person Type: {personType}");
        }

        /// Clicks the 'Continue' button.
        public async Task ClickContinue()
        {
            await _page.ClickAsync(ContinueButtonSelector);
        }

        /// Completes the form by selecting a person type and continuing.
        public async Task CompletePage(string selection)
        {
            await SelectPersonType(selection);
            await ClickContinue();
            Console.WriteLine($"✅ Completed Connected Person Type Page with selection: {selection}");
        }
    }
}
