using System.Threading.Tasks;
using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages
{
    public class ConnectedPersonsOrganisationCategoryPage
    {
        private readonly IPage _page;
        private readonly string _baseUrl;

        // ✅ Page Locators
        private readonly string PageTitleSelector = "h1.govuk-heading-l";
        private readonly string RegisteredCompanyRadioSelector = "input[name='organisationCategory'][value='registered-company']";
        private readonly string DirectorRadioSelector = "input[name='organisationCategory'][value='director']";
        private readonly string ParentSubsidiaryRadioSelector = "input[name='organisationCategory'][value='parent-subsidiary']";
        private readonly string TakenOverCompanyRadioSelector = "input[name='organisationCategory'][value='taken-over']";
        private readonly string OtherInfluenceRadioSelector = "input[name='organisationCategory'][value='other-influence']";
        private readonly string ContinueButtonSelector = "button.govuk-button[type='submit']";

        public ConnectedPersonsOrganisationCategoryPage(IPage page)
        {
            _page = page;
            _baseUrl = ConfigUtility.GetBaseUrl(); // Retrieve base URL from ConfigUtility
        }

        /// Navigates to the Connected Persons Organisation Category selection page using stored Organisation ID.
        public async Task NavigateTo(string storageKey)
        {
            string organisationId = StorageUtility.Retrieve(storageKey);
            if (string.IsNullOrEmpty(organisationId))
            {
                throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
            }

            string url = $"{_baseUrl}/organisation/{organisationId}/supplier-information/connected-person/organisation-category";
            await _page.GotoAsync(url);
            Console.WriteLine($"📌 Navigated to Connected Persons Organisation Category Page: {url}");

            await _page.WaitForSelectorAsync(PageTitleSelector);
        }

        /// Retrieves the page title to verify the correct page is loaded.
        public async Task<string> GetPageTitle()
        {
            await _page.WaitForSelectorAsync(PageTitleSelector);
            return await _page.InnerTextAsync(PageTitleSelector);
        }

        /// Selects an organisation category from the available choices.
        public async Task SelectOrganisationCategory(string category)
        {
            string radioSelector = category.ToLower() switch
            {
                "registered company" => RegisteredCompanyRadioSelector,
                "director" => DirectorRadioSelector,
                "parent or subsidiary" => ParentSubsidiaryRadioSelector,
                "taken over" => TakenOverCompanyRadioSelector,
                "other influence" => OtherInfluenceRadioSelector,
                _ => throw new System.Exception($"❌ Invalid organisation category: {category}. Must be 'registered company', 'director', 'parent or subsidiary', 'taken over', or 'other influence'.")
            };

            await _page.CheckAsync(radioSelector);
            Console.WriteLine($"✅ Selected Organisation Category: {category}");
        }

        /// Clicks the 'Continue' button.
        public async Task ClickContinue()
        {
            await _page.ClickAsync(ContinueButtonSelector);
        }

        /// Completes the form by selecting an organisation category and continuing.
        public async Task CompletePage(string category)
        {
            await SelectOrganisationCategory(category);
            await ClickContinue();
            Console.WriteLine($"✅ Completed Organisation Category Page with selection: {category}");
        }
    }
}
