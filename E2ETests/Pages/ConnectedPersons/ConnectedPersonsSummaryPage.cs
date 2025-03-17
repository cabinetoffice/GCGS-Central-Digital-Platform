using System.Threading.Tasks;
using Microsoft.Playwright;

namespace E2ETests.Pages
{
    public class ConnectedPersonsSummaryPage
    {
        private readonly IPage _page;

        // Locators for Change and Remove links
        private readonly string ChangeLinkSelector = "a:text('Change')";
        private readonly string RemoveLinkSelector = "a:text('Remove')";

        // Locators for Yes/No radio buttons
        private readonly string YesRadioSelector = "input[name='addAnotherConnectedPerson'][value='Yes']";
        private readonly string NoRadioSelector = "input[name='addAnotherConnectedPerson'][value='No']";

        // Locator for Continue button
        private readonly string ContinueButtonSelector = "button:text('Continue')";

        public ConnectedPersonsSummaryPage(IPage page)
        {
            _page = page;
        }

        /// Clicks the Change link for the first connected person.
        public async Task ClickChange()
        {
            await _page.ClickAsync(ChangeLinkSelector);
        }

        /// Clicks the Remove link for the first connected person.
        public async Task ClickRemove()
        {
            await _page.ClickAsync(RemoveLinkSelector);
        }

        /// Selects whether to add another connected person.
        public async Task SelectAddAnotherConnectedPerson(bool addAnother)
        {
            string radioButton = addAnother ? YesRadioSelector : NoRadioSelector;
            await _page.ClickAsync(radioButton);
        }

        /// Clicks the Continue button to proceed.
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
