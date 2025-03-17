using System.Threading.Tasks;
using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages
{
    public class ConnectedPersonsOrganisationNamePage
    {
        private readonly IPage _page;
        private readonly string _baseUrl;

        // ✅ Page Locators
        private readonly string PageTitleSelector = "h1.govuk-heading-l";
        private readonly string OrganisationNameInputSelector = "input.govuk-input[type='text']";
        private readonly string ContinueButtonSelector = "button.govuk-button[type='submit']";

        public ConnectedPersonsOrganisationNamePage(IPage page)
        {
            _page = page;
            _baseUrl = ConfigUtility.GetBaseUrl(); // Retrieve base URL from ConfigUtility
        }

        /// Navigates to the Connected Persons Organisation Name page using stored Organisation ID.
        public async Task NavigateTo(string storageKey)
        {
            string organisationId = StorageUtility.Retrieve(storageKey);
            if (string.IsNullOrEmpty(organisationId))
            {
                throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
            }

            string url = $"{_baseUrl}/organisation/{organisationId}/supplier-information/connected-person/organisation-name";
            await _page.GotoAsync(url);
            Console.WriteLine($"📌 Navigated to Connected Persons Organisation Name Page: {url}");

            await _page.WaitForSelectorAsync(PageTitleSelector);
        }

        /// Retrieves the page title to verify the correct page is loaded.
        public async Task<string> GetPageTitle()
        {
            await _page.WaitForSelectorAsync(PageTitleSelector);
            return await _page.InnerTextAsync(PageTitleSelector);
        }

        /// Enters the organisation's name into the input field.
        public async Task EnterOrganisationName(string organisationName)
        {
            await _page.FillAsync(OrganisationNameInputSelector, organisationName);
            Console.WriteLine($"✅ Entered Organisation Name: {organisationName}");
        }

        /// Clicks the 'Continue' button.
        public async Task ClickContinue()
        {
            await _page.ClickAsync(ContinueButtonSelector);
        }

        /// Completes the form by entering an organisation name and continuing.
        public async Task CompletePage(string organisationName)
        {
            await EnterOrganisationName(organisationName);
            await ClickContinue();
            Console.WriteLine($"✅ Completed Organisation Name Page with name: {organisationName}");
        }
    }
}
