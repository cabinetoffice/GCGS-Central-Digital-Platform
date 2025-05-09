using System.Threading.Tasks;
using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages
{
    public class TradeAssurancesWhoAwardedPage
    {
        private readonly IPage _page;
        private readonly string _baseUrl;
        private readonly string _formId;
        private readonly string _sectionId;

        // ✅ Page Locators
        private readonly string PageTitleSelector = "h1.govuk-fieldset__heading";
        private readonly string ContinueButtonSelector = "button:text('Continue')";
        private readonly string BackToSupplierInfoSelector = "a.govuk-back-link";
        private readonly string InputSelector = "input[name='TextInput'][type='text']";

        public TradeAssurancesWhoAwardedPage(IPage page)
        {
            _page = page;
            _baseUrl = ConfigUtility.GetBaseUrl(); // Retrieve base URL from ConfigUtility
        }

        /// Navigates to the Trade Assurances Yes/No page using stored Organisation ID.
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
            string questionId = "179d597c-3db2-41de-af9b-c651e64d486d";

            string url = $"{_baseUrl}/organisation/{organisationId}/forms/{formId}/sections/{sectionId}/question/{questionId}";
            await _page.GotoAsync(url);
            Console.WriteLine($"📌 Navigated to Trade Assurances Who Awarded Page: {url}");

            await _page.WaitForSelectorAsync(PageTitleSelector);
        }

        public async Task<string> GetPageTitle()
        {
            await _page.WaitForSelectorAsync(PageTitleSelector);
            return await _page.InnerTextAsync(PageTitleSelector);
        }

        public async Task InputWhoAwarded()
        {
            await _page.FillAsync(InputSelector, "John Johnson");
        }

        public async Task ClickContinue()
        {
            await _page.ClickAsync(ContinueButtonSelector);

        }

        public async Task ClickBackToSupplierInformation()
        {
            await _page.ClickAsync(BackToSupplierInfoSelector);
        }

        public async Task CompletePage()
        {
            await InputWhoAwarded();
            await ClickContinue();
        }

    }
}
