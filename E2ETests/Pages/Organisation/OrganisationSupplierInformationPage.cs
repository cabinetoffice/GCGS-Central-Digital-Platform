using System.Threading.Tasks;
using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages
{
    public class OrganisationSupplierInformationPage
    {
        private readonly IPage _page;
        private readonly string _baseUrl;

        // Locators
        private readonly string OrganisationNameSelector = "h1.govuk-heading-l";
        private readonly string BasicInformationLink = "//dt/a[contains(text(),'Basic information')]";
        private readonly string ConnectedPersonsLink = "//dt/a[contains(text(),'Connected persons')]";
        private readonly string QualificationsLink = "//dt/a[contains(text(),'Qualifications')]";
        private readonly string TradeAssurancesLink = "//dt/a[contains(text(),'Trade assurances')]";
        private readonly string ExclusionsLink = "//dt/a[contains(text(),'Exclusions')]";
        private readonly string FinancialInformationLink = "//dt/a[contains(text(),'Financial information')]";
        private readonly string ShareMyInformationLink = "//dt/a[contains(text(),'Share my information')]";
        private readonly string BackToOrganisationDetailsLink = "a.govuk-link[href*='/organisation']";

        public OrganisationSupplierInformationPage(IPage page)
        {
            _page = page;
            _baseUrl = ConfigUtility.GetBaseUrl();
        }

        /// Navigates to the Supplier Information page using a stored organisation ID.
        public async Task NavigateTo(string storageKey)
        {
            string organisationId = StorageUtility.Retrieve(storageKey);
            if (string.IsNullOrEmpty(organisationId))
            {
                throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
            }

            string url = $"{_baseUrl}/organisation/{organisationId}/supplier-information";
            await _page.GotoAsync(url);
            Console.WriteLine($"📌 Navigated to Supplier Information Page: {url}");

            await _page.WaitForSelectorAsync(OrganisationNameSelector);
        }

        /// Retrieves the Organisation Name from the page.
        public async Task<string> GetOrganisationName()
        {
            await _page.WaitForSelectorAsync(OrganisationNameSelector);
            return await _page.InnerTextAsync(OrganisationNameSelector);
        }

        /// Clicks the 'Basic Information' link.
        public async Task ClickBasicInformation()
        {
            await _page.ClickAsync(BasicInformationLink);
        }

        /// Clicks the 'Connected Persons' link.
        public async Task ClickConnectedPersons()
        {
            await _page.ClickAsync(ConnectedPersonsLink);
        }

        /// Clicks the 'Qualifications' link.
        public async Task ClickQualifications()
        {
            await _page.ClickAsync(QualificationsLink);
        }

        /// Clicks the 'Trade Assurances' link.
        public async Task ClickTradeAssurances()
        {
            await _page.ClickAsync(TradeAssurancesLink);
        }

        /// Clicks the 'Exclusions' link.
        public async Task ClickExclusions()
        {
            await _page.ClickAsync(ExclusionsLink);
        }

        /// Clicks the 'Financial Information' link.
        public async Task ClickFinancialInformation()
        {
            await _page.ClickAsync(FinancialInformationLink);
        }

        /// Clicks the 'Share My Information' link.
        public async Task ClickShareMyInformation()
        {
            await _page.ClickAsync(ShareMyInformationLink);
        }

        /// Clicks 'Back to Organisation Details' link.
        public async Task ClickBackToOrganisationDetails()
        {
            await _page.ClickAsync(BackToOrganisationDetailsLink);
        }
    }
}
