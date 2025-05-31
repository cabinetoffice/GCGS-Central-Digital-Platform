using System.Threading.Tasks;
using Microsoft.Playwright;

namespace E2ETests.Pages
{
    public class SupplierTypePage
    {
        private readonly IPage _page;

        // ✅ Page Locators
        private const string PageTitleSelector = "h1.govuk-fieldset__heading";
        private const string OrganisationOptionSelector = "input#organisation";
        private const string IndividualOptionSelector = "input#individual";
        private const string ContinueButtonSelector = "main >> button[type='submit']";

        public SupplierTypePage(IPage page)
        {
            _page = page;
        }

        public async Task<string> GetPageTitle()
        {
            return await _page.InnerTextAsync(PageTitleSelector);
        }

        public async Task SelectOrganisation()
        {
            await _page.ClickAsync(OrganisationOptionSelector);
            Console.WriteLine("✅ Selected 'Organisation'");
        }

        public async Task SelectIndividual()
        {
            await _page.ClickAsync(IndividualOptionSelector);
            Console.WriteLine("✅ Selected 'Individual'");
        }

        public async Task ClickContinue()
        {
            await _page.ClickAsync(ContinueButtonSelector);
            Console.WriteLine("✅ Clicked 'Continue' on Supplier Type Page");
        }

        public async Task CompletePage(string type)
        {
            if (type.ToLower() == "organisation")
                await SelectOrganisation();
            else if (type.ToLower() == "individual")
                await SelectIndividual();
            else
                throw new System.Exception($"❌ Invalid supplier type: '{type}'");

            await ClickContinue();
            Console.WriteLine("✅ Completed Supplier Type Page");
        }

        public async Task<bool> IsLoaded()
        {
            var title = await GetPageTitle();
            return title.Contains("Select which best describes your organisation");
        }
    }
}