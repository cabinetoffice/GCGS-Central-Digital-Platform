using System.Threading.Tasks;
using Microsoft.Playwright;

namespace E2ETests.Pages
{
    public class ConnectedPersonsSummaryPage
    {
        private readonly IPage _page;

        // ✅ Page Locators

        private readonly string ChangeLinkSelector = "a:text('Change')";
        private readonly string RemoveLinkSelector = "a:text('Remove')";

        private readonly string YesRadioSelector = "input[name='HasConnectedEntity'][value='true']";
        private readonly string NoRadioSelector = "input[name='HasConnectedEntity'][value='false']";

        private readonly string ContinueButtonSelector = "button:text('Continue')";

        public ConnectedPersonsSummaryPage(IPage page)
        {
            _page = page;
        }

        public async Task ClickChange()
        {
            await _page.ClickAsync(ChangeLinkSelector);
        }

        public async Task ClickRemove()
        {
            await _page.ClickAsync(RemoveLinkSelector);
        }

        public async Task SelectAddAnotherConnectedPerson(bool addAnother)
        {
            string radioButton = addAnother ? YesRadioSelector : NoRadioSelector;
            await _page.ClickAsync(radioButton);
        }

        public async Task ClickContinue()
        {
            await _page.ClickAsync(ContinueButtonSelector);
        }

        public async Task ClickBack()
        {
            await _page.ClickAsync("a.govuk-back-link");
            Console.WriteLine("✅ Clicked 'Back' on Summary Page");
        }


        /// Completes the page by selecting Yes/No for adding another connected person and clicking Continue.
        public async Task CompletePage(bool addAnother)
        {
            await SelectAddAnotherConnectedPerson(addAnother);
            await ClickContinue();
            Console.WriteLine("✅ Completed Connected Persons Summary Page");
        }
    }
}
