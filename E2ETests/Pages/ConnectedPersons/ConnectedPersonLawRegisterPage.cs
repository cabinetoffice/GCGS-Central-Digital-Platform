using System.Threading.Tasks;
using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages
{
    public class ConnectedPersonsLawRegisterPage
    {
        private readonly IPage _page;
        private readonly string _baseUrl;

        // ✅ Page Locators
        private readonly string PageTitleSelector = "h1.govuk-heading-l";
        private readonly string LegalFormationInputSelector = "input[name='legalFormation']";
        private readonly string LawEnforcementInputSelector = "input[name='lawEnforcement']";
        private readonly string ContinueButtonSelector = "button.govuk-button[type='submit']";

        public ConnectedPersonsLawRegisterPage(IPage page)
        {
            _page = page;
            _baseUrl = ConfigUtility.GetBaseUrl(); // Retrieve base URL from ConfigUtility
        }

        /// Navigates to the Connected Persons Law Register Page using stored Organisation ID.
        public async Task NavigateTo(string storageKey)
        {
            string organisationId = StorageUtility.Retrieve(storageKey);
            if (string.IsNullOrEmpty(organisationId))
            {
                throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
            }

            string url = $"{_baseUrl}/organisation/{organisationId}/supplier-information/connected-person/law-register";
            await _page.GotoAsync(url);
            Console.WriteLine($"📌 Navigated to Connected Persons Law Register Page: {url}");

            await _page.WaitForSelectorAsync(PageTitleSelector);
        }

        public async Task<string> GetPageTitle()
        {
            await _page.WaitForSelectorAsync(PageTitleSelector);
            return await _page.InnerTextAsync(PageTitleSelector);
        }

        public async Task EnterLegalFormation(string legalFormation)
        {
            await _page.FillAsync(LegalFormationInputSelector, legalFormation);
            Console.WriteLine($"✅ Entered Legal Formation: {legalFormation}");
        }

        public async Task EnterLawEnforcement(string lawEnforcement)
        {
            await _page.FillAsync(LawEnforcementInputSelector, lawEnforcement);
            Console.WriteLine($"✅ Entered Law Enforcement: {lawEnforcement}");
        }

        public async Task ClickContinue()
        {
            await _page.ClickAsync(ContinueButtonSelector);
        }

        /// Completes the form by entering details and clicking continue.
        public async Task CompletePage(string legalForm, string lawEnforced)
        {
            await EnterLegalFormation(legalForm);
            await EnterLawEnforcement(lawEnforced);
            await ClickContinue();
            Console.WriteLine($"✅ Completed Law Register Page with Legal Form: {legalForm}, Law: {lawEnforced}");
        }
    }
}
