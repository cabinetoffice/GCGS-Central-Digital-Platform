using System.Threading.Tasks;
using Microsoft.Playwright;

namespace E2ETests.Pages
{
    public class IndividualOrOrganisationPage
    {
        private readonly IPage _page;

        // ✅ Selectors
        private const string PageHeadingSelector = "h1.govuk-heading-l";
        private const string OrganisationRadioSelector = "input#organisation";
        private const string IndividualRadioSelector = "input#individual";
        private const string ContinueButtonSelector = "main >> button[type='submit']";

        public IndividualOrOrganisationPage(IPage page)
        {
            _page = page;
        }

        public async Task<string> GetPageHeading()
        {
            await _page.WaitForSelectorAsync(PageHeadingSelector);
            return await _page.InnerTextAsync(PageHeadingSelector);
        }

        public async Task SelectOrganisation()
        {
            await _page.ClickAsync(OrganisationRadioSelector);
            System.Console.WriteLine("✅ Selected 'Organisation'");
        }

        public async Task SelectIndividual()
        {
            await _page.ClickAsync(IndividualRadioSelector);
            System.Console.WriteLine("✅ Selected 'Individual'");
        }

        public async Task ClickContinue()
        {
            await _page.ClickAsync(ContinueButtonSelector);
            System.Console.WriteLine("✅ Clicked 'Continue' on Individual or Organisation Page");
        }

        public async Task CompletePage(bool isOrganisation)
        {
            if (isOrganisation)
                await SelectOrganisation();
            else
                await SelectIndividual();

            await ClickContinue();
            System.Console.WriteLine("✅ Completed Individual or Organisation Page");
        }
    }
}