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

        public async Task<string> GetOrganisationName()
        {
            await _page.WaitForSelectorAsync(OrganisationNameSelector);
            return await _page.InnerTextAsync(OrganisationNameSelector);
        }

        public async Task ClickBasicInformation()
        {
            await _page.ClickAsync(BasicInformationLink);
        }

        public async Task ClickConnectedPersons()
        {
            await _page.ClickAsync(ConnectedPersonsLink);
        }

        public async Task ClickQualifications()
        {
            await _page.ClickAsync(QualificationsLink);
        }

        public async Task ClickTradeAssurances()
        {
            await _page.ClickAsync(TradeAssurancesLink);
        }

        public async Task ClickExclusions()
        {
            await _page.ClickAsync(ExclusionsLink);
        }

        public async Task ClickFinancialInformation()
        {
            await _page.ClickAsync(FinancialInformationLink);
        }

        public async Task ClickShareMyInformation()
        {
            await _page.ClickAsync(ShareMyInformationLink);
        }

        public async Task ClickBackToOrganisationDetails()
        {
            await _page.ClickAsync(BackToOrganisationDetailsLink);
        }

        public async Task AssertConnectedPersonsCount(int expectedCount)
        {
            string expectedText = $"{expectedCount} added";
            string selector = "#connected-persons-status strong";

            await _page.WaitForSelectorAsync(selector);
            string actualText = await _page.InnerTextAsync(selector);

            if (actualText.Trim() != expectedText)
            {
                throw new System.Exception($"❌ Expected '{expectedText}' but found '{actualText}'");
            }

            Console.WriteLine($"✅ Connected persons count verified: {actualText}");
        }

    }
}
