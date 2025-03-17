using System.Threading.Tasks;
using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages
{
    public class OrganisationDetailsPage
    {
        private readonly IPage _page;
        private readonly string _baseUrl;

        // Locators for text values
        private readonly string OrganisationNameSelector = "//dt[contains(text(),'Organisation name')]/following-sibling::dd[@class='govuk-summary-list__value']/p";
        private readonly string OrganisationIdentifierSelector = "//dt[contains(text(),'Organisation identifier')]/following-sibling::dd[@class='govuk-summary-list__value']/p";
        private readonly string OrganisationEmailSelector = "//dt[contains(text(),'Organisation email')]/following-sibling::dd[@class='govuk-summary-list__value']/p";

        // Locators for Change/Add links
        private readonly string OrganisationChangeLink = "//dt[contains(text(),'Organisation name')]/following-sibling::dd[@class='govuk-summary-list__actions']/a";
        private readonly string OrganisationIdentifierAddLink = "//dt[contains(text(),'Organisation identifier')]/following-sibling::dd[@class='govuk-summary-list__actions']/a";
        private readonly string OrganisationEmailChangeLink = "//dt[contains(text(),'Organisation email')]/following-sibling::dd[@class='govuk-summary-list__actions']/a";

        // Other navigation links
        private readonly string CompleteSupplierInfoLink = "a[href*='/supplier-information']";
        private readonly string ManageUsersLink = "a[href*='/users/user-summary']";
        private readonly string RegisterAsBuyerLink = "a[href*='/register-supplier-as-buyer']";

        public OrganisationDetailsPage(IPage page)
        {
            _page = page;
            _baseUrl = ConfigUtility.GetBaseUrl(); // Get base URL from configuration utility
        }

        /// Navigates to the Organisation Details page using a stored organisation ID.
        public async Task NavigateTo(string storageKey)
        {
            // Retrieve the organisation ID from storage
            string organisationId = StorageUtility.Retrieve(storageKey);
            if (string.IsNullOrEmpty(organisationId))
            {
                throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
            }

            // Construct the URL dynamically
            string url = $"{_baseUrl}/organisation/{organisationId}";
            await _page.GotoAsync(url);
            Console.WriteLine($"📌 Navigated to Organisation Details: {url}");

            // Ensure the page is fully loaded
            await _page.WaitForSelectorAsync(OrganisationNameSelector);
        }

        /// Retrieves the Organisation Name from the page.
        public async Task<string> GetOrganisationName()
        {
            await _page.WaitForSelectorAsync(OrganisationNameSelector);
            return await _page.InnerTextAsync(OrganisationNameSelector);
        }

        /// Retrieves the Organisation Identifier from the page.
        public async Task<string> GetOrganisationIdentifier()
        {
            await _page.WaitForSelectorAsync(OrganisationIdentifierSelector);
            return await _page.InnerTextAsync(OrganisationIdentifierSelector);
        }

        /// Retrieves the Organisation Email from the page.
        public async Task<string> GetOrganisationEmail()
        {
            await _page.WaitForSelectorAsync(OrganisationEmailSelector);
            return await _page.InnerTextAsync(OrganisationEmailSelector);
        }

        /// Clicks the 'Change Organisation Name' link.
        public async Task ClickChangeOrganisationName()
        {
            await _page.ClickAsync(OrganisationChangeLink);
        }

        /// Clicks the 'Change Organisation Email' link.
        public async Task ClickChangeOrganisationEmail()
        {
            await _page.ClickAsync(OrganisationEmailChangeLink);
        }

        /// Clicks the 'Add Organisation Identifier' link.
        public async Task ClickAddOrganisationIdentifier()
        {
            await _page.ClickAsync(OrganisationIdentifierAddLink);
        }

        /// Clicks the 'Complete Supplier Information' link.
        public async Task ClickCompleteSupplierInfo()
        {
            await _page.ClickAsync(CompleteSupplierInfoLink);
        }

        /// Clicks the 'Manage Users' link.
        public async Task ClickManageUsers()
        {
            await _page.ClickAsync(ManageUsersLink);
        }

        /// Clicks the 'Register as a Buyer' link.
        public async Task ClickRegisterAsBuyer()
        {
            await _page.ClickAsync(RegisterAsBuyerLink);
        }
    }
}
