using System.Threading.Tasks;
using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages
{
    public class ConnectedPersonsSupplierCompanyQuestionPage
    {
        private readonly IPage _page;
        private readonly string _baseUrl;

        // ✅ Page Locators
        private readonly string PageTitleSelector = "h1.govuk-heading-l";
        private readonly string YesRadioSelector = "input[name='companiesHouse'][value='yes']";
        private readonly string NoRadioSelector = "input[name='companiesHouse'][value='no']";
        private readonly string ContinueButtonSelector = "button.govuk-button[type='submit']";

        public ConnectedPersonsSupplierCompanyQuestionPage(IPage page)
        {
            _page = page;
            _baseUrl = ConfigUtility.GetBaseUrl(); // Retrieve base URL from ConfigUtility
        }

        /// Navigates to the Connected Persons Supplier Company Question page using stored Organisation ID.
        public async Task NavigateTo(string storageKey)
        {
            string organisationId = StorageUtility.Retrieve(storageKey);
            if (string.IsNullOrEmpty(organisationId))
            {
                throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
            }

            string url = $"{_baseUrl}/organisation/{organisationId}/supplier-information/connected-person/supplier-company-question";
            await _page.GotoAsync(url);
            Console.WriteLine($"📌 Navigated to Connected Persons Supplier Company Question Page: {url}");

            await _page.WaitForSelectorAsync(PageTitleSelector);
        }

        /// Retrieves the page title to verify the correct page is loaded.
        public async Task<string> GetPageTitle()
        {
            await _page.WaitForSelectorAsync(PageTitleSelector);
            return await _page.InnerTextAsync(PageTitleSelector);
        }

        /// Selects either "Yes" or "No" for the Companies House Registration question.
        public async Task SelectCompanyRegistrationOption(bool isRegistered)
        {
            string radioSelector = isRegistered ? YesRadioSelector : NoRadioSelector;
            await _page.CheckAsync(radioSelector);
            Console.WriteLine($"✅ Selected Company Registration Option: {(isRegistered ? "Yes" : "No")}");
        }

        /// Clicks the 'Continue' button.
        public async Task ClickContinue()
        {
            await _page.ClickAsync(ContinueButtonSelector);
        }

        /// Completes the Connected Person Declaration Page.
        public async Task CompletePage(bool isRegistered)
        {
            await SelectCompanyRegistrationOption(isRegistered);
            await ClickContinue();
            Console.WriteLine("✅ Completed Connected Person Influence Page");
        }
    }
}