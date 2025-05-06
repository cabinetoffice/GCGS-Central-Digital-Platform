using System.Threading.Tasks;
using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages
{
    public class ConnectedPersonsCheckYourAnswersPage
    {
        private readonly IPage _page;
        private readonly string _baseUrl;

        // ✅ Page Locators
        private readonly string PageTitleSelector = "h1.govuk-heading-l";
        private readonly string CompanyNameChangeLink = "a[href*='/organisation-name']";
        private readonly string RegisteredAddressChangeLink = "a[href*='/Registered-address']";
        private readonly string PostalAddressChangeLink = "a[href*='/postal-address']";
        private readonly string LegalFormChangeLink = "a[href*='/law-register']";
        private readonly string CompaniesHouseChangeLink = "a[href*='/company-question']";
        private readonly string SubmitButtonSelector = "button.govuk-button[type='submit']";

        public ConnectedPersonsCheckYourAnswersPage(IPage page)
        {
            _page = page;
            _baseUrl = ConfigUtility.GetBaseUrl(); // Retrieve base URL from ConfigUtility
        }

        /// Navigates to the Connected Persons Check Your Answers Page using stored Organisation ID.
        public async Task NavigateTo(string storageKey)
        {
            string organisationId = StorageUtility.Retrieve(storageKey);
            if (string.IsNullOrEmpty(organisationId))
            {
                throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
            }

            string url = $"{_baseUrl}/organisation/{organisationId}/supplier-information/connected-person/check-answers-organisation";
            await _page.GotoAsync(url);
            Console.WriteLine($"📌 Navigated to Connected Persons Check Your Answers Page: {url}");

            await _page.WaitForSelectorAsync(PageTitleSelector);
        }

        public async Task<string> GetPageTitle()
        {
            await _page.WaitForSelectorAsync(PageTitleSelector);
            return await _page.InnerTextAsync(PageTitleSelector);
        }

        public async Task ClickChangeCompanyName()
        {
            await _page.ClickAsync(CompanyNameChangeLink);
            Console.WriteLine("✅ Clicked 'Change' for Company Name");
        }

        public async Task ClickChangeRegisteredAddress()
        {
            await _page.ClickAsync(RegisteredAddressChangeLink);
            Console.WriteLine("✅ Clicked 'Change' for Registered Address");
        }

        public async Task ClickChangePostalAddress()
        {
            await _page.ClickAsync(PostalAddressChangeLink);
            Console.WriteLine("✅ Clicked 'Change' for Postal Address");
        }

        public async Task ClickChangeLegalForm()
        {
            await _page.ClickAsync(LegalFormChangeLink);
            Console.WriteLine("✅ Clicked 'Change' for Legal Form");
        }

        public async Task ClickChangeCompaniesHouse()
        {
            await _page.ClickAsync(CompaniesHouseChangeLink);
            Console.WriteLine("✅ Clicked 'Change' for Companies House Registration");
        }

        public async Task ClickSubmit()
        {
            await _page.ClickAsync(SubmitButtonSelector);
            Console.WriteLine("✅ Clicked 'Submit' on Check Your Answers Page");
        }

        public async Task CompletePage()
        {
            await ClickSubmit();
            Console.WriteLine($"✅ Submitted Check Your Answers Page");
        }

        public async Task<bool> IsLoaded()
        {
            return await _page.Locator("h1").InnerTextAsync() == "Check your answers";
        }


    }
}
